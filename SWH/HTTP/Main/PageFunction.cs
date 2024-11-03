using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LunaHost.Attributes;
using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.Attributes.MiddleWares;
using LunaHost.HTTP.Interface;
using LunaHost.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LunaHost.HTTP.Main
{

    public abstract class PageContent : IDisposable
    {
        protected HttpRequest? request;
        const string _path = "";
        private string Path { get; set; }

        protected PageContent(string Path)
        {
            if (Path == null) throw new ArgumentNullException("path");
            if (!Path.StartsWith("/")) throw new ArgumentException("path must start with '/'.");
            this.Path = Path;

        }
        public PageContent()
        {
            this.Path = "/"+this.GetType().Name.ToLower();
        }
      
        public bool Match(HttpRequest req)
        {
            if (req == null) return false;

            if (req.Path == Path)
            {
                request = req;
            }
            else if (req.Path.StartsWith(Path))
            {
                request = req;
            }
            else
                return false;
            return true;
        }
        /// <summary>
        /// Tries to handle an HTTP request by finding the right page to show/reply with.
        /// </summary>
        /// <param name="httprequest">The HTTP request to handle. If you don’t give one, it will use the default.
        /// It is like switching to a new request </param>
        /// <param name="reset">If true, it will save the original request(default) and set it back before sending the respon</param>
        /// <returns>Returns an HTTP response.</returns>
        public IHttpResponse HandleRequest(HttpRequest httprequest =null!,bool reset=true)
        {
            if ((request == null && httprequest == null))
                return HttpResponse.NotFound(Path);
            HttpRequest reset_request = null!;
            if(httprequest != null &&!httprequest.Equals(request))
            {
                if(reset&& request is not null)
                {
                    reset_request = request;
                }
                request = httprequest;

            }
            MethodInfo Func = null!;
            IMethod method_attribute = null!;
            Dictionary<string,string> Route_values = new Dictionary<string,string>();
            foreach (var method in this.GetType().GetMethods().Where(x => x.ReturnType == typeof(IHttpResponse)))
            {
                method_attribute = null!;
                if (request.Method == HttpRequest.HttpMethod.GET)
                {
                    method_attribute = method.GetCustomAttribute<GetMethodAttribute>(true)!;
                }
                else if (request.Method == HttpRequest.HttpMethod.POST)
                {
                    method_attribute = method.GetCustomAttribute<PostMethodAttribute>(true)!;
                }
                else if (request.Method == HttpRequest.HttpMethod.PUT)
                {
                    method_attribute = method.GetCustomAttribute<PutMethodAttribute>(true)!;
                }
                else if (request.Method == HttpRequest.HttpMethod.DELETE)
                {
                    method_attribute = method.GetCustomAttribute<DeleteMethodAttribute>(true)!;
                }
                else
                {
                    break;
                }
                if (method_attribute == null)
                {
                    continue;
                }
                var m = method.GetCustomAttributes(true);
                // Проверки промежуточного ПО
                foreach (IMiddleWare Middleware in method.GetCustomAttributes(true).Where(x => x is IMiddleWare))
                {
                    var result = Middleware.ExcuteAsync(request, this.GetType()).Result;
                    if (result.successful)
                        continue;
                    return result.if_failed;
                }

            
                if (method_attribute.ContainsStaticValue)
                {
                    Dictionary<string, string> replacements = new Dictionary<string, string>();
                    
                    var static_url = Path + method_attribute.Path;
                    foreach (Match Key in Regex.Matches(method_attribute.Path, @"\{([a-zA-Z_][a-zA-Z0-9_]*)\}"))
                    {
                        string placeholderName = Key.Groups[1].Value;
                        string Route_Value = "";
                        if (string.Join("", static_url.Skip(static_url.IndexOf(Key.Value) + Key.Value.Length)) is "/" or "")
                        {
                            
                            Route_Value = string.Join("", request.Path.Skip(static_url.IndexOf(Key.Value)-1));
                            Route_Value = Route_Value.StartsWith('/') ? Route_Value.Substring(1) : Route_Value;

                        }
                        else
                        {
                            Route_Value = GetSegmentFromIndex(request.Path, static_url.IndexOf(Key.Value));

                        }

                        Route_values.Add(placeholderName, Route_Value);

                        replacements.Add($"{{{placeholderName}}}", Route_Value);
                        static_url = static_url.Replace('{' + placeholderName + '}', Route_Value);

                    }

                    foreach (var replacement in replacements)
                    {
                        method_attribute.Path = method_attribute.Path.Replace(replacement.Key, replacement.Value);
                    }
                }

                string cleanedGetAttPath = RemoveFirstInstanceOfSegment(method_attribute.Path ?? "//", "/");
                string cleanedRequestPath = RemoveFirstInstanceOfSegment(RemoveAfterQuestionMark(request.Path), RemoveFirstInstanceOfSegment(Path, "/"));

                if (cleanedGetAttPath != cleanedRequestPath && method_attribute.UrlType == UrlType.Match)
                {
                    continue; 
                }
                var e = RemoveFirstInstanceOfSegment(method_attribute.Path!, "/");
                if (method_attribute.UrlType == UrlType.Match)
                {
                    
                    Func = method;
                    break;
                }

                else if (method_attribute.UrlType == UrlType.After && cleanedRequestPath.StartsWith(e))
                {
                    Func = method;
                    break;
                }
            }

            
            if (Func == null)
            {
                var response = HttpResponse.NotFound("  Not Found " + request.Path);
                response.Headers["Content-Type"] = "application/json";
                return response;
            }
            var p_set = new List<object>() ;
            foreach (var item in Func.GetParameters())
            {
                if (item.GetCustomAttribute<FromRoute>(true) is FromRoute FR)
                {
                    if (FR.IsSet)
                    {
                        if (Route_values.TryGetValue(FR.Name ?? "", out string? routeValue))
                        {
                            p_set.Add(routeValue);
                        }
                        else
                        {
                            p_set.Add(item.DefaultValue ?? null!);
                        }
                    }
                    else
                    {
                        if (Route_values.TryGetValue(item.Name ?? "", out string? result))
                        {
                            p_set.Add(result);
                        }
                        else
                        {
                            p_set.Add(item.DefaultValue ?? null!);
                        }
                    }
                }
                else if (item.GetCustomAttribute<FromUrlQuery>(true) is FromUrlQuery FU)
                {
                    if (FU.IsSet)
                        p_set.Add(Uri.UnescapeDataString(FromUrl(GetQueryString(request.Path), FU.Name)));
                    else 
                    {
                        p_set.Add(Uri.UnescapeDataString(FromUrl(GetQueryString(request.Path),item.Name??"")));
                    }
                }
               
                else if (item.GetCustomAttribute<FromHeader>(true) is FromHeader FH)
                {
                    if (FH.IsSet)
                    {
                        if (request.Headers.TryGetValue(FH.Name ?? "", out string? headerValue))
                        {
                            p_set.Add(headerValue);
                        }
                        else
                        {
                            p_set.Add(item.DefaultValue ?? null!);
                        }
                    }
                    else
                    {
                        if (request.Headers.TryGetValue(item.Name ?? "", out string? result))
                        {
                            p_set.Add(result);
                        }
                        else
                        {
                            p_set.Add(item.DefaultValue!);
                        }
                    }

                }
                else if (item.GetCustomAttribute<FromBody>(true) is FromBody FB)
                {
                    try
                    {
                        JObject jsonObj = JObject.Parse(request.Body);

                      
                        if (FB.IsSet && !string.IsNullOrEmpty(FB.Name))
                        {
                            if (jsonObj.ContainsKey(FB.Name))
                            {
                             
                                var value = jsonObj[FB.Name]!.ToObject(item.ParameterType);
                                p_set.Add(value ?? item.DefaultValue ?? null!);
                            }
                            else
                            {
                               
                                p_set.Add(item.DefaultValue ?? null!);
                            }
                        }
                        else
                        {
                          
                            var deserializedValue = JsonConvert.DeserializeObject(request.Body, item.ParameterType);
                            p_set.Add(deserializedValue ?? request.Body ?? null!);
                        }
                    }
                    catch
                    {
                     
                        p_set.Add(request.Body);
                    }
                }
                else
                {
                    p_set.Add(item.DefaultValue!);
                }
            }
            p_set = p_set
                     .Select(item => item is string str ? str.Replace("\u0000", string.Empty) : item)
                     .ToList();
            if (reset)
                request = reset_request;
            return (HttpResponse)Func.Invoke(this,p_set.ToArray())!;



        }
        /// <summary>
        /// Retrieves a segment from the specified starting index in the path, stopping at the next '/' or '?' character.
        /// </summary>
        public string GetSegmentFromIndex(string path, int startIndex)
        {
            if (startIndex < 0 || startIndex >= path.Length)
            {
                return string.Empty;
            }
            int nextSlashIndex = path.IndexOf('/', startIndex);
            int nextQuestionMarkIndex = path.IndexOf('?', startIndex);
            int endIndex = -1;
            if (nextSlashIndex != -1 && nextQuestionMarkIndex != -1)
            {
                endIndex = Math.Min(nextSlashIndex, nextQuestionMarkIndex);
            }
            else if (nextSlashIndex != -1)
            {
                endIndex = nextSlashIndex;
            }
            else if (nextQuestionMarkIndex != -1)
            {
                endIndex = nextQuestionMarkIndex;
            }
            if (endIndex == -1)
            {
                return path.Substring(startIndex);
            }
            return path.Substring(startIndex, endIndex - startIndex);
        }
        /// <summary>
        /// Retrieves the query string from a URL, if present.
        /// </summary>
        static string GetQueryString(string input)
        {
            int questionMarkIndex = input.IndexOf('?');
            if (questionMarkIndex != -1 && questionMarkIndex < input.Length - 1)
            {
                return input.Substring(questionMarkIndex + 1);
            }
            return string.Empty; 
        }
        /// <summary>
        /// Extracts the value associated with a specified parameter name from a URL query string.
        /// </summary>
        /// <param name="fullUrl">The full URL containing query parameters.</param>
        /// <param name="Name">The name of the parameter to find.</param>
        /// <returns>The value of the specified parameter, or an empty string if the parameter is not found.</returns>
        static string FromUrl(string fullUrl,string Name)
        {
            var b_url = fullUrl.Split("&");
            var path = b_url.FirstOrDefault(x => x.TrimStart().StartsWith(Name));
            if (path != null)
            {
                return path.Split("=").LastOrDefault() ?? "";
            }
            return "";
        }
        /// <summary>
        /// Removes the first occurrence of the specified segment from the input string.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="segment">The segment to remove.</param>
        /// <returns>The modified string with the first occurrence of the segment removed, or the original string if not found.</returns>
        static string RemoveFirstInstanceOfSegment(string input, string segment)
        {
            int index = input.IndexOf(segment);
            if (index == -1)
            {
                return input;
            }
            string before = input.Substring(0, index).TrimEnd('/');
            string after = input.Substring(index + segment.Length).TrimStart('/');
            return before.Length > 0 && after.Length > 0 ? $"{before}/{after}" : before + after;
        }

        /// <summary>
        /// Removes everything in the input string after the first question mark (or "/?").
        /// </summary>
        /// <param name="input">The string to process. Could be a URL or any string with a query part.</param>
        /// <returns>The part of the string before the first question mark, or the whole string if no question mark is found.</returns>
        static string RemoveAfterQuestionMark(string input)
        {
            int index = input.IndexOf("/?");
            if (index == -1)
            {
                index = input.IndexOf("?");
            }

            return index == -1 ? input : input.Substring(0, index);
        }

       

        public void Dispose()
        {
            GC.SuppressFinalize(this);

        }
    }
}
