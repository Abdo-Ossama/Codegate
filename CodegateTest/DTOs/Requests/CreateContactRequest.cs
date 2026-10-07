using System.ComponentModel.DataAnnotations;

namespace CodegateTest.DTOs.Requests
{
    public class CreateContactRequest
    {
        [Required, StringLength(100, MinimumLength = 3)]
        public string SenderName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Phone, StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(150)]
        public string Subject { get; set; } = string.Empty;

        [Required, StringLength(5000, MinimumLength = 10)]
        public string Message { get; set; } = string.Empty;
    }
}
