using System.ComponentModel.DataAnnotations;

namespace CodegateTest.DTOs.Requests
{
    public class ResetPasswordRequest
    {

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [MaxLength(100, ErrorMessage = "Password can't exceed 100 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and Confirm Password do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty; // Hidden Field
    }
}
