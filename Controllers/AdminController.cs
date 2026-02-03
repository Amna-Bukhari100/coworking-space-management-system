using CoWorkManager.Models;
using CoWorkManager.Models.Interfaces;
using CoWorkManager.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoWorkManager.Controllers
{
    [Authorize(Policy = "AdminPolicy")]
    public class AdminController : Controller
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IVisitorRepository _visitorRepo;
        private readonly IWorkspaceRepository _workspaceRepo;
        private readonly IHubContext<BookingHub> _hubContext;

        public AdminController(
            IBookingRepository bookingRepo,
            IVisitorRepository visitorRepo,
            IWorkspaceRepository workspaceRepo,
            IHubContext<BookingHub> hubContext)
        {
            _bookingRepo = bookingRepo;
            _visitorRepo = visitorRepo;
            _workspaceRepo = workspaceRepo;
            _hubContext = hubContext;
        }

        public IActionResult Dashboard()
        {
            var bookings = _bookingRepo.GetAll() ?? Enumerable.Empty<Booking>();
            var workspaces = _workspaceRepo.GetAll() ?? Enumerable.Empty<Workspace>();
            var visitors = _visitorRepo.GetAll() ?? Enumerable.Empty<Visitor>();

            ViewBag.TotalWorkspaces = workspaces.Count();
            ViewBag.TotalBookings = bookings.Count();
            ViewBag.TotalVisitors = visitors.Count();

            ViewBag.SimpleChartData = new List<int> {
                workspaces.Count(),
                bookings.Count(),
                visitors.Count()
            };

            return View(bookings.ToList());
        }

        public IActionResult Bookings()
        {
            var bookings = _bookingRepo.GetAll() ?? Enumerable.Empty<Booking>();
            return View(bookings.ToList());
        }

        public IActionResult Visitors()
        {
            var visitors = _visitorRepo.GetAll() ?? Enumerable.Empty<Visitor>();
            return View(visitors.ToList());
        }

        public IActionResult ManageWorkspaces()
        {
            var workspaces = _workspaceRepo.GetAll() ?? Enumerable.Empty<Workspace>();
            return View(workspaces.ToList());
        }

        [HttpPost]
        public IActionResult ToggleWorkspaceStatus(int id)
        {
            var workspace = _workspaceRepo.GetById(id);
            if (workspace != null)
            {
                workspace.IsAvailable = !workspace.IsAvailable;
                _workspaceRepo.Update(workspace);
            }
            return RedirectToAction(nameof(ManageWorkspaces));
        }

        [HttpGet]
        public IActionResult AddWorkspace() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddWorkspace(Workspace workspace)
        {
            if (ModelState.IsValid)
            {
                _workspaceRepo.Add(workspace);
                return RedirectToAction(nameof(ManageWorkspaces));
            }
            return View(workspace);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBooking(int id)
        {
            var booking = _bookingRepo.GetById(id);
            if (booking != null)
            {
                booking.BookingStatus = "Confirmed";
                _bookingRepo.Update(booking);

                // SignalR call
                await _hubContext.Clients.All.SendAsync("ShowNotification", 
                    $"Booking #{id} has been confirmed!");
            }
            return RedirectToAction(nameof(Bookings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminCancelBooking(int id)
        {
            var booking = _bookingRepo.GetById(id);
            if (booking != null)
            {
                booking.BookingStatus = "Cancelled";
                _bookingRepo.Update(booking);

                await _hubContext.Clients.All.SendAsync("ShowNotification",
                    $"Booking #{id} has been cancelled by administrator.");
            }
            return RedirectToAction(nameof(Bookings));
        }
    }
}