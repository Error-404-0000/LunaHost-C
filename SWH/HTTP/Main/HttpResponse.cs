using System.Text;
using LunaHost.HTTP.Interface;

namespace LunaHost.HTTP.Main
{
    public class HttpResponse : IHttpResponse
    {
        public int StatusCode { get; set; }
        public string ReasonPhrase { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string Body { get; set; }

        public HttpResponse()
        {
            SetStatus(200, "OK");
            Headers["Content-Type"] = "text/html+application/json";
        }

        // Set the HTTP Status Code and Reason Phrase
        public void SetStatus(int code, string reasonPhrase)
        {
            StatusCode = code;
            ReasonPhrase = reasonPhrase;
        }

        // Set the response body and automatically set content length
        public void SetBody(string content, string contentType = "text/html")
        {
            Body = content;
            Headers["Content-Type"] = contentType;
            Headers["Content-Length"] = Encoding.UTF8.GetByteCount(content).ToString();
        }

        public string GetFullResponse()
        {
            StringBuilder responseBuilder = new StringBuilder();

         
            responseBuilder.AppendLine($"HTTP/1.1 {StatusCode} {ReasonPhrase}");

            
            foreach (var header in Headers)
            {
                responseBuilder.AppendLine($"{header.Key}: {header.Value}");
            }

            responseBuilder.AppendLine();

            if (!string.IsNullOrEmpty(Body))
            {
                responseBuilder.Append(Body);
            }

            return responseBuilder.ToString();
        }

        public static HttpResponse OK(string body = "OK")
        {
            var response = new HttpResponse();

            response.SetStatus(200, "OK");
            response.SetBody(body);
            return response;
        }

        public static HttpResponse NotFound(string body = "404 Not Found")
        {
            var response = new HttpResponse();
            response.SetStatus(404, "Not Found");
            response.SetBody(body);
            return response;
        }

        public static HttpResponse InternalServerError(string body = "500 Internal Server Error")
        {
            var response = new HttpResponse();
            response.SetStatus(500, "Internal Server Error");
            response.SetBody(body);
            return response;
        }
        public static HttpResponse BadRequest(string body = "400 Bad Request")
        {
            var response = new HttpResponse();
            response.SetStatus(400, "Bad Request");
            response.SetBody(body);
            return response;
        }

        public static HttpResponse Unauthorized(string body = "401 Unauthorized")
        {
            var response = new HttpResponse();
            response.SetStatus(401, "Unauthorized");
            response.SetBody(body);
            return response;
        }

        public static HttpResponse Forbidden(string body = "403 Forbidden")
        {
            var response = new HttpResponse();
            response.SetStatus(403, "Forbidden");
            response.SetBody(body);
            return response;
        }

        public static HttpResponse MethodNotAllowed(string body = "405 Method Not Allowed")
        {
            var response = new HttpResponse();
            response.SetStatus(405, "Method Not Allowed");
            response.SetBody(body);
            return response;
        }

        public static HttpResponse Conflict(string body = "409 Conflict")
        {
            var response = new HttpResponse();
            response.SetStatus(409, "Conflict");
            response.SetBody(body);
            return response;
        }

        public static HttpResponse Gone(string body = "410 Gone")
        {
            var response = new HttpResponse();
            response.SetStatus(410, "Gone");
            response.SetBody(body);
            return response;
        }

        public static HttpResponse UnsupportedMediaType(string body = "415 Unsupported Media Type")
        {
            var response = new HttpResponse();
            response.SetStatus(415, "Unsupported Media Type");
            response.SetBody(body);
            return response;
        }

        public static HttpResponse TooManyRequests(string body = "429 Too Many Requests")
        {
            var response = new HttpResponse();
            response.SetStatus(429, "Too Many Requests");
            response.SetBody(body);
            return response;
        }

        public static HttpResponse ServiceUnavailable(string body = "503 Service Unavailable")
        {
            var response = new HttpResponse();
            response.SetStatus(503, "Service Unavailable");
            response.SetBody(body);
            return response;
        }

    }

}
