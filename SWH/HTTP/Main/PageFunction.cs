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
using CacheLily;
using LunaHost.MiddleWares;
using CacheLily.Attributes;

namespace LunaHost.HTTP.Main
{
#if DEBUG
    [NoCaching]
#endif
    public abstract class PageContent : IDisposable,ICacheable
    {
        protected HttpRequest? request;
     
        public string Path { get; }
        public  int CacheCode { get; set; }

        protected PageContent(string Path)
        {
            if (Path == null) throw new ArgumentNullException("path");
            if (!Path.StartsWith("/")) throw new ArgumentException("Path(s) must start with '/'.");
            this.Path = Path;

        }
        public PageContent()
        {
            if(this.GetType().GetCustomAttribute<RouteAttribute>() is RouteAttribute RA)
            {
                if(!RA.Route.StartsWith("/")) throw new ArgumentException("Path(s) must start with '/'.");
                this.Path = RA.Route;
                return;
            }
            if (this.GetType().Name is string s && s.EndsWith("Content") && s.Length>7)
            {
                this.Path="/"+ s.Remove(s.Length-7).ToLower();
                return;
            }
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
        public   IHttpResponse HandleRequest(HttpRequest httprequest =null!,bool reset=true)
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
#pragma warning disable
            var result = InvokeMiddleWareAsync(this.GetType().GetCustomAttributes(true).Where(x => x is IMiddleWare).Cast<IMiddleWare>(),this).Result;
            if (!result.Success)
                return result.Response;
#pragma warning restore
            MethodInfo Func = null!;
            IMethod method_attribute = null!;
            Dictionary<string,string> Route_values = new Dictionary<string,string>();
            foreach (var method in this.GetType().GetMethods().Where(x => x.ReturnType == typeof(IHttpResponse)))
            {
                if (request is null)
                    break;
                method_attribute = null!;
                if (request!?.Method == HttpRequest.HttpMethod.GET)
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


                if (method_attribute.ContainsStaticValue)
                {
                    Dictionary<string, string> replacements = new Dictionary<string, string>();
                    
                    var static_url = Path + method_attribute.Path;
                    foreach (Match Key in Regex.Matches(method_attribute.Path, @"\{(.*?)\}"))
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
                if (method_attribute.Path == "/")
                    method_attribute.Path = "//";
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
#pragma warning disable

            if (Func != null)
            {
                var respon = InvokeMiddleWareAsync(Func.GetCustomAttributes(true).Where(x => x is IMiddleWare).Cast<IMiddleWare>(), Func).Result;
                if (!respon.Success)
                    return respon.Response;
            }
#pragma warning


            if (Func == null)
            {
                var response = HttpResponse.NotFound("Not Found " + request?.Path??"");
                response.Headers["Content-Type"] = "application/json";
                return response;
            }
            var p_set = new List<object>() ;
            foreach (var item in Func.GetParameters())
            {
                
                object? paramValue = item.DefaultValue;

                if (item.GetCustomAttribute<FromRoute>(true) is FromRoute routeAttr)
                {
                    paramValue = GetRouteValue(Route_values,routeAttr, item);
                }
                else if (item.GetCustomAttribute<FromUrlQuery>(true) is FromUrlQuery queryAttr)
                {
                    paramValue = GetQueryValue(queryAttr, item);
                }
                else if (item.GetCustomAttribute<FromHeader>(true) is FromHeader headerAttr)
                {
                    paramValue = GetHeaderValue(headerAttr, item);
                }
                else if (item.GetCustomAttribute<FromBody>(true) is FromBody bodyAttr)
                {
                    paramValue = GetBodyValue(bodyAttr, item, request!);
                }
              
                paramValue ??= item.ParameterType == typeof(string) ? "" : default;
                paramValue = paramValue.GetType() == typeof(DBNull) ? default : paramValue;
                result = InvokeMiddleWareAsync(item.GetCustomAttributes(true).Where(x => x is IMiddleWare).Cast<IMiddleWare>(), paramValue).Result;
                if (!result.Success)
                    return result.Response;
                p_set.Add(paramValue??(item.HasDefaultValue?item.DefaultValue:Convert.ChangeType(null,item.ParameterType)));
            }
            p_set = p_set
                     .Select(item => item is string str ? str.Replace("\u0000", string.Empty) : item)
                     .ToList();
            if (reset)
                request = reset_request;
            return (HttpResponse)Func.Invoke(this,p_set.ToArray())!;



        }

        public async Task<IMiddleWareResult<IHttpResponse>> InvokeMiddleWareAsync(IEnumerable<IMiddleWare> MiddleWares,object from)
        {
            if (MiddleWares is null)
                goto ret;
            dynamic prefer_v = null;
            foreach (var MiddleWare in MiddleWares)
            {
                
                if (from is not null && from.GetType().GetCustomAttribute<NoPreferences>() is not null)
                    goto call;
                switch (MiddleWare.GetType().GetCustomAttribute<NoPreferences>())
                {
                    case null:
                        goto preferences_check;
                    default:
                        goto call;
                }

                preferences_check:
                switch (MiddleWare.GetType().GetMethod(nameof(MiddleWare.ExecuteAsync)).GetCustomAttribute<NoPreferences>())
                {
                    case null:
                        var pref = (MiddleWare.GetType().GetMethod("ExecuteAsync").GetParameters().
                            Where(x => x.GetCustomAttributes<ObjectPrefer>().Any())?.FirstOrDefault()?.GetCustomAttribute<ObjectPrefer>() as ObjectPrefer);
                        if (pref is null)
                            goto call;
                        switch (pref.preferred)
                        {
                            case Enums.Preferred.None:
                                goto call;
                            case Enums.Preferred.Method:
                                prefer_v = from is MethodInfo ? from : throw new Exception($"{from.GetType()} does not match the Preferred Value.");
                            break;
                            case Enums.Preferred.Property:
                                 prefer_v = from is PropertyInfo ? from : throw new Exception($"{from.GetType()} does not match the Preferred Value.");
                                break;
                            //case Enums.Preferred.Parameter:
                            //    prefer_v = from is ParameterInfo ? from : throw new Exception($"{from.GetType()} does not match the Preferred Value.");
                            //    break;
                            //case Enums.Preferred.ParameterValue:
                            //    prefer_v = from is ParameterInfo e? e.DefaultValue : throw new Exception($"{from.GetType()} does not match the Preferred Value.");
                            //    break;
                            //any preferred / Value
                            default:
                                prefer_v = from;
                            break;
                        }
                        goto call;
                    break;
                }
             call:
                if(await MiddleWare.ExecuteAsync(request!,prefer_v) is IMiddleWareResult<IHttpResponse> mid_re && !mid_re.Success)
                    return mid_re;

                ctr:
                continue;
            }
            ret:
            return new MiddleWareResult<IHttpResponse>(HttpResponse.OK(), true);
        }

        /// <summary>
        /// Get the value from route
        /// </summary>
        /// <param name="Route_values"></param>
        /// <param name="routeAttr"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        object? GetRouteValue(Dictionary<string,string> Route_values, FromRoute routeAttr, ParameterInfo item)
        {
            if (routeAttr.IsSet && Route_values.TryGetValue(routeAttr.Name, out string? routeValue))
            {
                return ConvertToType(routeValue, item.ParameterType);
            }
            else if (!routeAttr.IsSet && Route_values.TryGetValue(item.Name, out string? rV))
            {
                return ConvertToType(rV, item.ParameterType);

            }

            return item.DefaultValue;
        }

        /// <summary>
        /// Get the value from a UrlQuery
        /// </summary>
        /// <param name="queryAttr"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        object? GetQueryValue(FromUrlQuery queryAttr, ParameterInfo item)
        {
            string queryString = null;
            if (queryAttr.IsSet)
                 queryString = Uri.UnescapeDataString(FromUrl(GetQueryString(request.Path), queryAttr.Name));
            else
                queryString = Uri.UnescapeDataString(FromUrl(GetQueryString(request.Path), item.Name));

            return ConvertToType(queryString, item.ParameterType);
        }
        // Helper method for FromHeader
        object? GetHeaderValue(FromHeader headerAttr, ParameterInfo item)
        {
            if (headerAttr.IsSet&& request.Headers.TryGetValue(headerAttr.Name , out var headerValue))
            {
                return ConvertToType(headerValue, item.ParameterType);
            }
            else if ( request.Headers.TryGetValue(item.Name, out var HV))
            {
                return ConvertToType(HV, item.ParameterType);
            }
            return item.DefaultValue;
        }

        /// <summary>
        /// Convert the Body to a C# item
        /// </summary>
        /// <param name="bodyAttr"></param>
        /// <param name="item"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        object? GetBodyValue(FromBody bodyAttr, ParameterInfo item, HttpRequest request)
        {
            bool isValidJson = false;
            try
            {
                JObject jsonObj = JObject.Parse(request.Body);
                isValidJson = true;
                if (bodyAttr.IsSet && jsonObj.ContainsKey(bodyAttr.Name))
                {
                    return jsonObj[bodyAttr.Name]!.ToObject(item.ParameterType);
                }
                else if (!bodyAttr.IsSet && jsonObj.ContainsKey(item.Name))
                {
                    return jsonObj[item.Name]!.ToObject(item.ParameterType);
                }
                return JsonConvert.DeserializeObject(request.Body, item.ParameterType) ?? request.Body;
            }
            catch
            {
               if(isValidJson)
                    return item.ParameterType == typeof(string) ? "" : default;
                else
                return request.Body;
            }
        }
        object? ConvertToType(object? value, Type targetType)
        {
            if (value == DBNull.Value || value is string s && s is "")
            {
                return default(object); 
            }
            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                return default;
            }
            
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
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(segment))
            {
                return input;
            }

            int index = input.IndexOf(segment, StringComparison.Ordinal);
            if (index == -1)
            {
                return input;
            }

            try
            {
                string before = input.Substring(0, index).TrimEnd('/');
                string after = input.Substring(index + segment.Length).TrimStart('/');
                return before.Length > 0 && after.Length > 0 ? $"{before}/{after}" : before + after;
            }
            catch
            {
                return input; // Safe fallback in case of unexpected error
            }
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
