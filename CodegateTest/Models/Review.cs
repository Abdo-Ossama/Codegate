namespace CodegateTest.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = null!;
        public string Feedback { get; set; } = null!;
        public int Rating { get; set; }
        public bool IsApproved { get; set; }
        public int CourseId { get; set; }

        public Course Course { get; set; } = null!;
    }
}
