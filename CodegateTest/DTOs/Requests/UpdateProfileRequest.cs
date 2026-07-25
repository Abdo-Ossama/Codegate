namespace CodegateTest.DTOs.Requests
{
    public class UpdateProfileRequest
    {
        public string? Fname { get; set; } = string.Empty;
        public string ?Lname { get; set; } = string.Empty;
        public IFormFile? ProfileImage { get; set; }
    }
}
