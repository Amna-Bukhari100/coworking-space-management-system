using Microsoft.AspNetCore.Mvc;
using CoWorkManager.Models.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace CoWorkManager.ViewComponents
{
    public class PendingBookingsViewComponent : ViewComponent
    {
        private readonly IBookingRepository _bookingRepo;

        public PendingBookingsViewComponent(IBookingRepository bookingRepo)
        {
            _bookingRepo = bookingRepo;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Keep your existing synchronous repository call
            var allBookings = _bookingRepo.GetAll();
            
            var count = allBookings.Count(b => b.BookingStatus == "Pending");
            
            return await Task.FromResult(View(count));
        }
    }
}