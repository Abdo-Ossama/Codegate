namespace CodegateTest.DTOs.Requests
{
    public class UpdateReviewRequest
    {
        public string Feedback { get; set; } = string.Empty;

        public int Rating { get; set; }
    }
}