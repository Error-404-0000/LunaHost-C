using System.Text;
using CacheLily;
using LunaHost.HTTP.Interface;
using Newtonsoft.Json;

namespace LunaHost.HTTP.Main
{
    public class HttpResponse : IHttpResponse
    {
        public int StatusCode { get; set; }
        public string ReasonPhrase { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string Body { get; set; } // For string-based bodies
        public byte[] BinaryBody { get; private set; } // For raw binary data

        public int CacheCode { get; set; }

        public HttpResponse(string contentType = "text/html; charset=utf-8")
        {
            SetStatus(200, "OK");
            Headers["Content-Type"] = contentType;
        }

        // Set the HTTP Status Code and Reason Phrase
        public void SetStatus(int code, string reasonPhrase)
        {
            StatusCode = code;
            ReasonPhrase = reasonPhrase;
        }

        // Set the response body as a string and automatically set content length
        public void SetBody(string content, string contentType = "text/html; charset=utf-8")
        {
            Body = content;
            BinaryBody = null; // Clear any previously set binary body
            Headers["Content-Type"] = contentType;
            Headers["Content-Length"] = Encoding.UTF8.GetByteCount(content).ToString();
        }

        // Set the response body as raw bytes
        public void SetBody(byte[] content, string contentType = "application/octet-stream")
        {
            if (content is null)
                return;
            BinaryBody = content;
            Body = null; // Clear any previously set string body
            Headers["Content-Type"] = contentType;
            Headers["Content-Length"] = content.Length.ToString();
        }

        // Generate the full HTTP response as a string for textual content
        //public string GetFullResponse()
        //{
        //    StringBuilder responseBuilder = new StringBuilder();

        //    // Start with the HTTP status line
        //    responseBuilder.AppendLine($"HTTP/1.1 {StatusCode} {ReasonPhrase}");

        //    // Add headers
        //    foreach (var header in Headers)
        //    {
        //        responseBuilder.AppendLine($"{header.Key}: {header.Value}");
        //    }

        //    // Add a blank line to separate headers and body
        //    responseBuilder.AppendLine();

        //    // Append the body if it is a string
        //    if (!string.IsNullOrEmpty(Body))
        //    {
        //        responseBuilder.Append(Body);
        //    }

        //    return responseBuilder.ToString();
        //}

        // Generate the full HTTP response as raw bytes
        public string GetFullResponse()
        {
            StringBuilder headerBuilder = new StringBuilder();

            // Start with the HTTP status line
            headerBuilder.AppendLine($"HTTP/1.1 {StatusCode} {ReasonPhrase}");

            // Add headers
            foreach (var header in Headers)
            {
                headerBuilder.AppendLine($"{header.Key}: {header.Value}");
            }

            // Add a blank line to separate headers and body
            headerBuilder.AppendLine();

            // Convert headers to string
            string headersAsString = headerBuilder.ToString();

            // Check if there is binary body
            if (BinaryBody != null)
            {
                // Convert binary data to Base64
                string binaryBodyBase64 = Convert.ToBase64String(BinaryBody);

                // Add a marker to indicate Base64-encoded content
                return headersAsString + "Base64-Encoded-Content-Start\n" + binaryBodyBase64;
            }

            // If the body is string-based, append it directly to the headers
            if (!string.IsNullOrEmpty(Body))
            {
                return headersAsString + Body;
            }

            // Return headers only if no body is present
            return headersAsString;
        }





        // Factory methods for common HTTP responses
        public static HttpResponse OK(string body = "OK",string contentType = null)
        {
            var response = new HttpResponse();
            response.SetStatus(200, "OK");
            if(contentType is not null)
                response.SetBody(body,contentType);
            else
                response.SetBody(body);
            return response;
        }

        public static HttpResponse OK(byte[] body, string contentType = "application/octet-stream")
        {
            var response = new HttpResponse();
            response.SetStatus(200, "OK");
            response.SetBody(body, contentType);
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

        // Add other factory methods as needed
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
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
