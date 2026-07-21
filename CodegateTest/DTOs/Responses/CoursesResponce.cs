namespace CodegateTest.DTOs.Responses
{
    public class CoursesResponce
    {
        public Object items { get; set; } = null!;
        public int totalPages { get; set; }
        public int totalCourses { get; set; }
        public int pageSize { get; set; }
      
    }
}
