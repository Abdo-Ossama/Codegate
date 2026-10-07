using System.ComponentModel.DataAnnotations;

namespace CodegateTest.DTOs.Requests
{
    public class CreateUserRequest
    {
        [Required]
        [StringLength(256)]
        [RegularExpression(@"^[a-zA-Z0-9@._+\-]+$",
            ErrorMessage = "Username contains invalid characters.")]
        public string UserName { get; set; } = string.Empty;

        [Required, StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required, RegularExpression("^(Admin|Student)$")]
        public string Role { get; set; } = string.Empty;
    }
}
