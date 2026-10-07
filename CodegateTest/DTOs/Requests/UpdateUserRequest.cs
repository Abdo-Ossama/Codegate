using System.ComponentModel.DataAnnotations;

namespace CodegateTest.DTOs.Requests
{
    public class UpdateUserRequest
    {
        [StringLength(256, MinimumLength = 1)]
        [RegularExpression(@"^[a-zA-Z0-9@._+\-]+$",
            ErrorMessage = "Username contains invalid characters.")]
        public string? UserName { get; set; }

        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[\s\S]*\S[\s\S]*$", ErrorMessage = "First name cannot contain only whitespace.")]
        public string ?FirstName { get; set; }

        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[\s\S]*\S[\s\S]*$", ErrorMessage = "Last name cannot contain only whitespace.")]
        public string? LastName { get; set; }

        [EmailAddress]
        [StringLength(255)]
        public string ?Email { get; set; }

        [StringLength(100, MinimumLength = 8)]
        [RegularExpression(@"^[\s\S]*\S[\s\S]*$", ErrorMessage = "Password cannot contain only whitespace.")]
        public string? Password { get; set; }

        [MinLength(1), RegularExpression("^(Admin|Student)$")]
        public string? Role { get; set; }
    }
}
