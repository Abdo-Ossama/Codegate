namespace CodegateTest.DTOs.Responses
{
    public class APIResponce
    {
        public string UUID { get; set; } = Guid.NewGuid().ToString();
        public int StatusCode { get; set; }
        public string[]? Message { get; set; }
        public object ? Data { get; set; } 
        public DateTime DateTime { get; set; } = DateTime.Now;
    }
}
