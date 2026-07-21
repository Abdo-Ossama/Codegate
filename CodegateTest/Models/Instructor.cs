using System.ComponentModel.DataAnnotations;

namespace CodegateTest.Models
{
    public class Instructor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "First Name must be between 2 and 50 characters.")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "Last Name must be between 2 and 50 characters.")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "Title must be between 3 and 100 characters.")]
        public string Title { get; set; } = null!;

        public string? AvatarUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        public ICollection<CourseInstructors> CourseInstructors { get; set; }
        = new List<CourseInstructors>();
    }
}