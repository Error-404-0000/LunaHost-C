namespace LunaHost_Test.Visitors
{
    public class RequestInformation
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public RequestType RequestType {  get; set; }
        public RequestInformation Response { get; set; }
    }
}
