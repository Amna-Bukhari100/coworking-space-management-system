using System.Collections.Generic;

namespace CoWorkManager.Models
{
    public class Workspace
    {
        public int WorkspaceId { get; set; }

        public string? Name { get; set; }
        public string? Type { get; set; }    // Meeting Room, Desk, Office
        public decimal PricePerHour { get; set; }

        public bool IsAvailable { get; set; }

        public List<Booking>? Bookings { get; set; }
    }
}