using System.ComponentModel.DataAnnotations;

namespace CodegateTest.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Course name is required.")]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "Course name must be between 3 and 100 characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Slug is required.")]
        [StringLength(150)]
        [RegularExpression(@"^[a-z0-9-]+$",
            ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens.")]
        public string Slug { get; set; } = null!;

        [Required(ErrorMessage = "Price is required.")]
       
        public decimal Price { get; set; }

     
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? CoverImageUrl { get; set; }

        public ICollection<CourseInstructors> CourseInstructors { get; set; }
             = new List<CourseInstructors>();
    }
}