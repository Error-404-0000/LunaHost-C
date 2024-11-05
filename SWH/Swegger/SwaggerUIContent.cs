using LunaHost.Attributes;
using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;

namespace Swegger
{
    public sealed class SwaggerUIContent : PageContent
        {
            private readonly string _swaggerUiPath;

            public SwaggerUIContent(string swaggerUiPath) : base("/docs")
            {
                _swaggerUiPath = swaggerUiPath;
            }

            [GetMethod]
            public IHttpResponse ServeUI([FromUrlQuery("file")] string? file = "index.html")
            {
                var filePath = System.IO.Path.Combine(_swaggerUiPath, file);
            
                if (File.Exists(filePath))
                {
                    var contentType = file.EndsWith(".css") ? "text/css" :
                                      file.EndsWith(".js") ? "application/javascript" :
                                      file.EndsWith(".html") ? "text/html" : "text/plain";

                    return new HttpResponse
                    {
                        Body = File.ReadAllText(filePath),
                        StatusCode = 200,
                        Headers = { ["Content-Type"] = contentType }
                    };
                }
                else
                {
                    return new HttpResponse
                    {
                        Body = "File not found",
                        StatusCode = 404
                    };
                }
            }
        }
    

}
