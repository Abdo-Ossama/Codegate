using System.ComponentModel.DataAnnotations;

namespace CodegateTest.DTOs.Requests
{
    public class InstructorCreateRequest
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string? AvatarUrl { get; set; }
    }
}
