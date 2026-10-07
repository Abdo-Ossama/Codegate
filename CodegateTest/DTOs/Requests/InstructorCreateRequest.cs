using System.ComponentModel.DataAnnotations;

namespace CodegateTest.DTOs.Requests
{
    public class InstructorCreateRequest
    {
        [Required, StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = null!;

        [Required, StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = null!;

        [Required, StringLength(100, MinimumLength = 3)]
        public string Title { get; set; } = null!;

      
    }
}
