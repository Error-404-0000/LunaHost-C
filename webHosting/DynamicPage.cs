using SWH.Attributes;
using SWH.Attributes.HttpMethodAttributes;
using SWH.HTTP.Interface;
using SWH.HTTP.Main;
using SWH.MiddleWares;
namespace webHosting
{
    internal partial class Program
    {
        public class DynamicPage : PageContent
        {
            public List<(string path,string content,string Content_Type)> pages  = new();
            /// <summary>
            /// Creates a new page. at run time
            /// </summary>
            /// <param name="path">The path, e.g., host/{path}, must come from the header and be part of the route in the URL</param>
            /// <param name="Content_Type">The content type</param>
            /// <returns>An HTTP response indicating the result of the operation</returns>
            [RequiredHeader("Content-Type")]
            [PostMethod("/create/page")]
            public IHttpResponse CreatePage([ FromHeader] string path, [FromHeader("Content-Type")] string Content_Type)
            {
                if (string.IsNullOrEmpty(path))
                    return HttpResponse.BadRequest();
                if (pages.Any(x => x.path == path))
                    return HttpResponse.BadRequest("Page already created.");

                pages.Add(new(path, request!.Body, Content_Type));
                return HttpResponse.OK($"Created {path}");
            }

            /// <summary>
            /// Retrieves an existing page.
            /// </summary>
            /// <param name="path">The path, e.g., host/{path}, must be part of the route in the URL;path can be a long path like /path1/path/2........</param>
            /// <returns>An HTTP response containing the page content</returns>
            [GetMethod("/{path}")]
            public IHttpResponse GetPage([FromRoute] string path)
            {
                if (string.IsNullOrEmpty(path))
                    return HttpResponse.BadRequest();
                if (pages.Any(x => x.path == path))
                {
                    var p = pages.FirstOrDefault(x => x.path == path);
                    IHttpResponse resp = HttpResponse.OK(p.content);
                    resp.Headers["Content-Type"] = p.Content_Type;
                    return resp;
                }
                return HttpResponse.NotFound();
            }

        }
       
        
    }

 
    
}
