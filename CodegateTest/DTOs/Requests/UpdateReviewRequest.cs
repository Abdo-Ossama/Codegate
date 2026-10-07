using System.ComponentModel.DataAnnotations;

namespace CodegateTest.DTOs.Requests
{
    public class UpdateReviewRequest
    {
        [Required, StringLength(2000, MinimumLength = 3)]
        public string Feedback { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }
    }
}
