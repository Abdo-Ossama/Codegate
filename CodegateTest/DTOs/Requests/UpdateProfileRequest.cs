using System.ComponentModel.DataAnnotations;

namespace CodegateTest.DTOs.Requests
{
    public class UpdateProfileRequest
    {
        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[\s\S]*\S[\s\S]*$", ErrorMessage = "First name cannot contain only whitespace.")]
        public string? Fname { get; set; }
        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[\s\S]*\S[\s\S]*$", ErrorMessage = "Last name cannot contain only whitespace.")]
        public string? Lname { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
}
