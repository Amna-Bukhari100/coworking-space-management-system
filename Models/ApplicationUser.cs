using Microsoft.AspNetCore.Identity;

namespace CoWorkManager.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
