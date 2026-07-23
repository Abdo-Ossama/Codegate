using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CodegateTest.DTOs.Requests
{
    public class CreateCourseRequest
    {
        [Required(ErrorMessage = "Course name is required.")]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Slug is required.")]
        [StringLength(150)]
        [RegularExpression(@"^[a-z0-9-]+$",
            ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens.")]
        public string Slug { get; set; } = null!;

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Cover Image is required.")]
        public IFormFile CoverImage { get; set; } = null!;

        [Required(ErrorMessage = "At least one instructor is required.")]
        [MinLength(1, ErrorMessage = "At least one instructor is required.")]
        public List<int> InstructorIds { get; set; } = new();
    }
}