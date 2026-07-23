namespace CodegateTest.DTOs.Responses
{
    public class UserResponse
    {
        public Object items { get; set; } = null!;
        public int totalPages { get; set; }
        public int totalUsers { get; set; }
        public int pageSize { get; set; }
    }
}