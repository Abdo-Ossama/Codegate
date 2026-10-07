using System.ComponentModel.DataAnnotations;

namespace CodegateTest.DTOs.Requests
{
    public class InstructorUpdateRequest
    {
        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[\s\S]*\S[\s\S]*$", ErrorMessage = "First name cannot contain only whitespace.")]
        public string? FirstName { get; set; }
        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[\s\S]*\S[\s\S]*$", ErrorMessage = "Last name cannot contain only whitespace.")]
        public string? LastName { get; set; }
        [StringLength(100, MinimumLength = 3)]
        [RegularExpression(@"^[\s\S]*\S[\s\S]*$", ErrorMessage = "Title cannot contain only whitespace.")]
        public string? Title { get; set; }
    }
}
