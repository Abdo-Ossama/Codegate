using Microsoft.AspNetCore.Identity;

namespace CodegateTest.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Fname { get; set; } = string.Empty;
        public string Lname { get; set; } = string.Empty;
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    }
}
