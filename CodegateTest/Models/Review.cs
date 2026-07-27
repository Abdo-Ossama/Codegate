namespace CodegateTest.Models
{
    namespace CodegateTest.Models
    {
        public enum ReviewStatus
        {
            Pending,
            Approved,
            Rejected
        }
        public class Review
        {
            public int Id { get; set; }

            public string Feedback { get; set; } = string.Empty;

            public int Rating { get; set; }

            public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.Pending;

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            public DateTime? UpdatedAt { get; set; }

         
            public string StudentId { get; set; } = string.Empty;

            public ApplicationUser Student { get; set; } = null!;

            public int CourseId { get; set; }

            public Course Course { get; set; } = null!;
        }
    }
}
