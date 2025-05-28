namespace LunaHost_Test.Visitors
{
    public class RequestInformation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Url { get; set; }
        public string Title { get; set; }
        public string Method { get; set; }
        public string RawBody { get; set; }
        public RequestType RequestType { get; set; }
        public Dictionary<string,string> Headers { get; set; } // Added for better logging
        public int StatusCode { get; set; } // Added to capture response status codes
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Added timestamp for each request
        public RequestInformation Response { get; set; }
    }

}
