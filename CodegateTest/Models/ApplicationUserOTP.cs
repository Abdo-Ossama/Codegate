namespace CodegateTest.Models
{
    public class ApplicationUserOTP
    {
        public int Id { get; set; }
        public string OTP { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiredAt { get; set; } = DateTime.UtcNow.AddMinutes(10);
        public bool IsValid => ExpiredAt > DateTime.Now; //  Check على الExpiredAt

        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
