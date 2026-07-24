namespace CodegateTest.DTOs.Responses
{
    public class ContactResponce
    {
        public int Page { get; set; }
        public int TotalMessagesCount { get; set; }
        public int TotalPages { get; set; }
        public List<Contact> items { get; set; } = null!;
    }
}
