namespace CodegateTest.Models
{
    public class CourseInstructors
    {
        public int Id { get; set; }

        public int CourseId { get; set; }

        public int InstructorId { get; set; }

        public Course Course { get; set; } = null!;

        public Instructor Instructor { get; set; } = null!;
    }
}
