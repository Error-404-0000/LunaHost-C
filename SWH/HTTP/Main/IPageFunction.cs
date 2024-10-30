using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SWH.Attributes;
using SWH.Attributes.HttpMethodAttributes;
using SWH.Attributes.MiddleWares;
using SWH.HTTP.Interface;
using SWH.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SWH.HTTP.Main
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
        /// 
        /// </summary>
        /// <param name="force_search">if the Handler should still try to find the route 
        /// even if the start does not match</param>
        /// <returns></returns>
        public IHttpResponse HandleRequest(bool force_search = false)
        {
            if (request == null && !force_search)
                return HttpResponse.NotFound(Path);

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

            return (HttpResponse)Func.Invoke(this,p_set.ToArray())!;



        }
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

        static string GetQueryString(string input)
        {
            int questionMarkIndex = input.IndexOf('?');
            if (questionMarkIndex != -1 && questionMarkIndex < input.Length - 1)
            {
                return input.Substring(questionMarkIndex + 1);
            }
            return string.Empty; 
        }
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
