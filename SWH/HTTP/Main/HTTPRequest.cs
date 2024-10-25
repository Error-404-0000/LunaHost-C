using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWH
{

    public partial class HttpRequest
    {

        // Properties
        public HttpMethod Method { get; private set; }
        public string Path { get; private set; }
        public string HttpVersion { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
        public string Body { get; private set; }
        public HttpRequest(string rawRequest)
        {
            Headers = new Dictionary<string, string>();
            ParseRequest(rawRequest);
            if(Path!= null && Headers.ContainsKey("Host"))
                     Path = Path.Replace(Headers["Host"], "");

        }

        // Parsing the raw HTTP request string
        private void ParseRequest(string rawRequest)
        {
            
           
            string[] lines = rawRequest.Split(new[] { "\r\n" }, StringSplitOptions.None);

            // The first line is the request line: Method, Path, HTTP Version
            string[] requestLine = lines[0].Replace("http://","")
                .Replace("https://","").Split(' ');
            Method = ParseHttpMethod(requestLine[0]);
            Path = requestLine.Length > 1 ? requestLine[1] : "/";
            HttpVersion = requestLine.Length > 2 ? requestLine[2] : "HTTP/1.1";

            // Parse headers (lines after the request line and before the empty line)
            int i = 1;
            for (; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    // Reached an empty line, headers are done
                    break;
                }

                string[] headerParts = line.Split(new[] { ": " }, 2, StringSplitOptions.None);
                if (headerParts.Length == 2)
                {
                    Headers[headerParts[0]] = headerParts[1];
                }
            }

            // If there is a body (it comes after the empty line)
            if (i < lines.Length - 1)
            {
                Body = string.Join("\r\n", lines[(i + 1)..]);
            }
           
        }

        // Parsing the HTTP method string into an enum
        private HttpMethod ParseHttpMethod(string method)
        {
            return method switch
            {
                "GET" => HttpMethod.GET,
                "POST" => HttpMethod.POST,
                "PUT" => HttpMethod.PUT,
                "DELETE" => HttpMethod.DELETE,
                "HEAD" => HttpMethod.HEAD,
                "OPTIONS" => HttpMethod.OPTIONS,
                "PATCH" => HttpMethod.PATCH,
                _ => HttpMethod.UNKNOWN
            };
        }

        // ToString override to easily view the parsed HTTP request
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Method: {Method}");
            builder.AppendLine($"Path: {Path}");
            builder.AppendLine($"HTTP Version: {HttpVersion}");
            builder.AppendLine("Headers:");
            foreach (var header in Headers)
            {
                builder.AppendLine($"  {header.Key}: {header.Value}");
            }
            if (!string.IsNullOrWhiteSpace(Body))
            {
                builder.AppendLine("Body:");
                builder.AppendLine(Body);
            }
            return builder.ToString();
        }
    }


}
