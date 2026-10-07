namespace CodegateTest.DTOs.Requests
{
    public class LoginRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string UserName { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required]
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
