using System;
using System.Collections.Generic;

namespace CoWorkManager.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        public List<Visitor> Visitors { get; set; } = new List<Visitor>();

        public int WorkspaceId { get; set; }
        public Workspace? Workspace { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string? BookingStatus { get; set; } // Pending, Confirmed, Cancelled

        public string? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
    }
}