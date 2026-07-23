namespace CodegateTest.DTOs.Requests
{
    public class CourseUpdateRequest
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        
        public IFormFile? CoverImg { get; set; }

        public List<int>? InstructorIds { get; set; }
    }
}

