namespace SWH.HTTP.Interface
{
    public interface IHttpResponse
    {
        int StatusCode { get; set; }
        string ReasonPhrase { get; set; }
        Dictionary<string, string> Headers { get; set; }
        string Body { get; set; }

        void SetStatus(int code, string reasonPhrase);
        void SetBody(string content, string contentType = "text/html");
        string GetFullResponse();
    }

}
