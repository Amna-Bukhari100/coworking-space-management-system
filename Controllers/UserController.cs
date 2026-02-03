using CoWorkManager.Models;
using CoWorkManager.Models.Interfaces;
using CoWorkManager.Hubs; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoWorkManager.Controllers
{
    [Authorize(Policy = "UserPolicy")]
    public class UserController : Controller
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IVisitorRepository _visitorRepo;
        private readonly IWorkspaceRepository _workspaceRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<BookingHub> _hubContext; 

        public UserController(
            IBookingRepository bookingRepo,
            IVisitorRepository visitorRepo,
            IWorkspaceRepository workspaceRepo,
            UserManager<ApplicationUser> userManager,
            IHubContext<BookingHub> hubContext) 
        {
            _bookingRepo = bookingRepo;
            _visitorRepo = visitorRepo;
            _workspaceRepo = workspaceRepo;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var allBookings = _bookingRepo.GetAll() ?? Enumerable.Empty<Booking>();
            var myBookings = allBookings.Where(b => b.ApplicationUserId == user.Id).ToList();

            ViewBag.TotalBookings = myBookings.Count;
            var workspaces = _workspaceRepo.GetAll() ?? Enumerable.Empty<Workspace>();
            ViewBag.TotalWorkspaces = workspaces.Count(w => w.IsAvailable);

            return View(myBookings);
        }

        public async Task<IActionResult> MyBookings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var allBookings = _bookingRepo.GetAll() ?? Enumerable.Empty<Booking>();
            var myBookings = allBookings.Where(b => b.ApplicationUserId == user.Id).ToList();

            return View(myBookings);
        }

        public async Task<IActionResult> VisitorLog()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var myBookingIds = _bookingRepo.GetAll()
                .Where(b => b.ApplicationUserId == user.Id)
                .Select(b => b.BookingId)
                .ToList();

            var myVisitors = _visitorRepo.GetAll()
                .Where(v => myBookingIds.Contains(v.BookingId))
                .ToList();

            ViewBag.ActiveBookingId = myBookingIds.FirstOrDefault();

            return View(myVisitors);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterGuestAjax([FromBody] Visitor visitor)
        {
            if (ModelState.IsValid)
            {
                _visitorRepo.Add(visitor);

                // SIMPLIFIED: One SignalR call with clear message
                await _hubContext.Clients.All.SendAsync("ShowNotification", 
                    $"New guest registered: {visitor.FullName} for Booking #{visitor.BookingId}");

                return Json(new { 
                    success = true, 
                    fullName = visitor.FullName, 
                    email = visitor.Email, 
                    phone = visitor.Phone, 
                    bookingId = visitor.BookingId 
                });
            }

            var errorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
            return Json(new { success = false, message = errorMessage ?? "Invalid data provided." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var booking = _bookingRepo.GetById(id);

            if (booking != null && booking.ApplicationUserId == user?.Id && booking.BookingStatus == "Pending")
            {
                _bookingRepo.Delete(id);
                
                await _hubContext.Clients.All.SendAsync("ShowNotification",
                    $"Booking #{id} cancelled by user.");
                
                TempData["Message"] = "Booking cancelled successfully.";
            }
            else
            {
                TempData["Error"] = "Only pending bookings can be cancelled.";
            }
            return RedirectToAction(nameof(MyBookings));
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ApplicationUser model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Message"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Profile));
            }
            return View("Profile", user);
        }

        [HttpGet]
        public IActionResult CreateBooking()
        {
            ViewBag.Workspaces = _workspaceRepo.GetAll().Where(w => w.IsAvailable).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBooking(Booking booking)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            bool isOccupied = _bookingRepo.IsWorkspaceOccupied(booking.WorkspaceId, booking.StartTime, booking.EndTime);
            if (isOccupied)
            {
                ModelState.AddModelError("", "This workspace is already booked for the selected time.");
            }

            ModelState.Remove("Visitors");
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("Workspace");

            if (ModelState.IsValid)
            {
                booking.ApplicationUserId = user.Id;
                booking.BookingStatus = "Pending";
                _bookingRepo.Add(booking);

                // SIMPLIFIED: Clear notification
                await _hubContext.Clients.All.SendAsync("ShowNotification",
                    $"New booking request #{booking.BookingId} submitted for {booking.Workspace?.Name}");

                return RedirectToAction(nameof(MyBookings));
            }

            ViewBag.Workspaces = _workspaceRepo.GetAll().Where(w => w.IsAvailable).ToList();
            return View(booking);
        }

        [HttpGet]
        public async Task<IActionResult> RegisterGuest(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);
            var booking = _bookingRepo.GetById(bookingId);

            if (booking == null || booking.ApplicationUserId != user?.Id || booking.BookingStatus != "Confirmed")
            {
                return RedirectToAction(nameof(MyBookings));
            }

            ViewBag.GuestList = _visitorRepo.GetByBookingId(bookingId);
            ViewBag.WorkspaceName = booking.Workspace?.Name;
            ViewBag.BookingId = bookingId;
            return View(new Visitor { BookingId = bookingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterGuest(Visitor visitor)
        {
            if (ModelState.IsValid)
            {
                _visitorRepo.Add(visitor);

                await _hubContext.Clients.All.SendAsync("ShowNotification",
                    $"Guest {visitor.FullName} registered for Booking #{visitor.BookingId}");

                return RedirectToAction(nameof(RegisterGuest), new { bookingId = visitor.BookingId });
            }
            return View(visitor);
        }
    }
}