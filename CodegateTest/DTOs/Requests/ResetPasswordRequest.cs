using System.ComponentModel.DataAnnotations;
namespace CodegateTest.DTOs.Requests
{


    public class ResetPasswordRequest
    {
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        public string ResetToken { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
