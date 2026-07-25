using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CodegateTest.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Fname { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Lname { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; }

        public string ProfileImageUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}