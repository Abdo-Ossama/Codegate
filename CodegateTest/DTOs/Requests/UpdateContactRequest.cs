using System.ComponentModel.DataAnnotations;

namespace CodegateTest.DTOs.Requests
{
    public class UpdateContactRequest
    {
        [StringLength(100, MinimumLength = 3)]
        [RegularExpression(@"^[\s\S]*\S[\s\S]*$", ErrorMessage = "Sender name cannot contain only whitespace.")]
        public string? SenderName { get; set; }

        [EmailAddress, StringLength(255)]
        public string? Email { get; set; }

        [Phone, StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(150)]
        public string? Subject { get; set; }

        [StringLength(5000, MinimumLength = 10)]
        [RegularExpression(@"^[\s\S]*\S[\s\S]*$", ErrorMessage = "Message cannot contain only whitespace.")]
        public string? Message { get; set; }
    }
}
