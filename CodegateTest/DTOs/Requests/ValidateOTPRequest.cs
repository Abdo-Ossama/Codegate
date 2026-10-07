namespace CodegateTest.DTOs.Requests
{
    public class ValidateOTPRequest
    {
     
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^[0-9]{4}$",
            ErrorMessage = "OTP must contain exactly four digits.")]
        public string OTP { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required]
        public string ApplicationUserId { get; set; } = string.Empty; // Hidden Feild
    }
}
