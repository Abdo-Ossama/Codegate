namespace CodegateTest.DTOs.Requests
{
    public class CreateReviewRequest
    {
        public int CourseId { get; set; }

        public string Feedback { get; set; } = string.Empty;

        public int Rating { get; set; }
    }
}
