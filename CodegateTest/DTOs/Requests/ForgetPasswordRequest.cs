namespace CodegateTest.DTOs.Requests
{
    public class ForgetPasswordRequest
    {
     
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string Email { get; set; } = string.Empty;
    }
}
