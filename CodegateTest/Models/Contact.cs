using System.ComponentModel.DataAnnotations;

namespace CodegateTest.Models
{
    public class Contact
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Sender name is required.")]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "Sender name must be between 3 and 100 characters.")]
        public string SenderName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(255)]
        public string Email { get; set; } = null!;

        [Phone(ErrorMessage = "Invalid phone number.")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(150,
            ErrorMessage = "Subject cannot exceed 150 characters.")]
        public string? Subject { get; set; }

        [Required(ErrorMessage = "Message is required.")]
        [StringLength(5000, MinimumLength = 10,
            ErrorMessage = "Message must be between 10 and 5000 characters.")]
        public string Message { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}