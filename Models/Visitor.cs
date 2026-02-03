using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Required for validation

namespace CoWorkManager.Models
{
    public class Visitor
    {
        public int VisitorId { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        public string? FullName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Phone number must be exactly 11 digits.")]
        public string? Phone { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }
    }
}