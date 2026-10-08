namespace CodegateTest.Models
{
    public class ApplicationUserOTP
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; } = null!;

        public ApplicationUser ApplicationUser { get; set; } = null!;

        public string OTPHash { get; set; } = null!;

        public bool IsUsed { get; set; }

        public int FailedAttempts { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiredAt { get; set; }
    }
}
