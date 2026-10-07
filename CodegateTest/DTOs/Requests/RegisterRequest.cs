using System.ComponentModel.DataAnnotations;

namespace CodegateTest.DTOs.Requests
{
    public class RegisterRequest
    {

        [Required, StringLength(50, MinimumLength = 2)]
        public string Fname { get; set; } = string.Empty;
        [Required, StringLength(50, MinimumLength = 2)]
        public string Lname { get; set; } = string.Empty;
        [Required, EmailAddress, StringLength(255)]
        public string Email { get; set; } = string.Empty;
        [Required, StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;
        [Required, StringLength(256)]
        [RegularExpression(@"^[a-zA-Z0-9@._+\-]+$", ErrorMessage = "Username contains invalid characters.")]
        public string UserName { get; set; } = string.Empty;

        [Required, Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;

    }
}
