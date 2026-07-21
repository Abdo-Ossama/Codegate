namespace CodegateTest.DTOs.Requests
{
    public class CourseCreateRequest
    {
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public string? CoverImageUrl { get; set; }

        public List<int> InstructorIds { get; set; } = new();
    }
}
