using System.ComponentModel.DataAnnotations;

namespace CodegateTest.DTOs.Requests
{
    public class CourseUpdateRequest
    {
        [StringLength(100, MinimumLength = 3)]
        [RegularExpression(@"^[\s\S]*\S[\s\S]*$", ErrorMessage = "Course name cannot contain only whitespace.")]
        public string? Name { get; set; }
        [StringLength(150, MinimumLength = 1)]
        [RegularExpression(@"^[a-z0-9-]+$")]
        public string? Slug { get; set; }
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335",
            ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true,
            ErrorMessage = "Price must be at least 0.01.")]
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        
        public IFormFile? CoverImg { get; set; }

        public List<int>? InstructorIds { get; set; }
    }
}

