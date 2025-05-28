using LunaHost;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost_Test.Db;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using LunaHost_Test.Visitors;
using Newtonsoft.Json;

namespace LunaHost_Test.Routes
{
    public class RequestHandler : IDisposable
    {
        private readonly HttpClient _client;
        private readonly string ProxyDomain = "http://10.0.0.71"; // Proxy base domain
        private const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36";
        private HttpRequest request;

        public RequestHandler(HttpRequest request)
        {
            this.request = request;

            // Bypass SSL certificate errors (for local testing)
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.All,


                ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
            };

            _client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            _client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            _client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        }

        private RequestType StringToRequest(string method)
        {
            return method.ToUpper() switch
            {
                "POST" => RequestType.POST,
                "PUT" => RequestType.PUT,
                "GET" => RequestType.GET,
                "DELETE" => RequestType.DELETE,
                _ => RequestType.Other
            };
        }
        private (bool shouldlog, Guid? id) VisitorControl(ref Configuration config, string requestPath)
        {
            bool log = false;
            var Req = new Visitors.RequestInformation();
            if (config.TargetPathLogs.Any(x => x.Path == "*" && x.LogType == LogType.Log) || config.TargetPathLogs.Count == 0)
            {
                var vistor = (config.Visitors.FirstOrDefault(x => x.IP == request.SourceAddress));
                if (vistor != null)
                {
                    if (vistor.DontLog)
                    {
                        return (false, null);
                    }
                    else
                    {
                        Req.Url = requestPath;
                        Req.RawBody = request.Body;
                        Req.Title = requestPath;
                        Req.Headers = request.Headers;
                        Req.RequestType = StringToRequest(nameof(request.Method));
                        vistor.RequestInformation.Add(Req);
                        return (true, Req.Id);
                    }
                }
                else
                {
                    var v = new Visitor()
                    {
                        FirstViste = DateTime.Now,
                        LastUrl = requestPath,
                        LastViste = DateTime.Now,
                        IP = request.SourceAddress,
                        DontLog = false,

                    };
                    Req.Url = requestPath;
                    Req.RawBody = request.Body;
                    Req.Headers = request.Headers;
                    Req.RequestType = StringToRequest(nameof(request.Method));
                    Req.Title = requestPath;
                    v.RequestInformation.Add(Req);
                    config.Visitors.Add(v);
                    return (true, Req.Id);

                }


            }
            else if (config.TargetPathLogs.Any(x => x.Path.Contains(requestPath) && x.LogType == LogType.Log) || config.TargetPathLogs.Count == 0)
            {
                var vistor = (config.Visitors.FirstOrDefault(x => x.IP == request.SourceAddress));
                if (vistor != null)
                {
                    if (vistor.DontLog)
                    {
                        return (false, null);
                    }
                    else
                    {
                        Req.Url = requestPath;
                        Req.RawBody =request.Body;
                        Req.Title = requestPath;
                        Req.Headers = request.Headers;
                        Req.RequestType = StringToRequest(nameof(request.Method));
                        vistor.RequestInformation.Add(Req);
                        return (true, Req.Id);
                    }
                }
                else
                {
                    var v = new Visitor()
                    {
                        FirstViste = DateTime.Now,
                        LastUrl = requestPath,
                        LastViste = DateTime.Now,
                        IP = request.SourceAddress,
                        DontLog = false,

                    };
                    Req.Url = requestPath;
                    Req.RawBody = request.Body;
                    Req.Title = requestPath;
                    v.RequestInformation.Add(Req);
                    config.Visitors.Add(v);
                    Req.Headers = request.Headers;
                    Req.RequestType = StringToRequest(nameof(request.Method));
                    return (true, Req.Id);

                }


            }
            return (false, null);
        }
        private bool IsStandAloneDomain = false;
        public IHttpResponse ProcessRequest(string configId, string requestPath, string method)
        {
            if (request is null)
                return HttpResponse.InternalServerError();

            // Decode inbound path, restore ? and &
            requestPath = requestPath
                .Replace("__Q__", "?")
                .Replace("__D__", "&");
            var config = AppDb.Configurations.FirstOrDefault(x => x.Id.ToString().Replace("-", "") == configId);

            if (config == null)
                return HttpResponse.NotFound();
            var Logrespon = Task.Run(() => VisitorControl(ref config, requestPath));
            var originalUrl = BuildUrl(config.Domain, requestPath);

            var forwardedRequest = new HttpRequestMessage(new HttpMethod(method), originalUrl);

            foreach (var header in request.Headers)
            {
                if (!string.Equals(header.Key, "Host", StringComparison.OrdinalIgnoreCase))
                {
                    forwardedRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
            if(request.Headers.ContainsKey("Referer"))
            {
                request.Headers.Remove("Referer");
                //var @ref = request.Path.Split("/");
                //if(@ref.Length > 2)
                //{
                //   var h = string.Join("/", @ref.Skip(3));
                //    if(h.StartsWith("http"))
                //    request.Headers["Referer"]= string.Join("/", @ref.Skip(3));
                //    else
                //        request.Headers["Referer"] = "https://" + string.Join("/", @ref.Skip(3));
                //}
                //else
                //{
                //    request.Headers["Referer"] = config.Domain;
                //}

            }
            forwardedRequest.Headers.UserAgent.ParseAdd(DefaultUserAgent);

            if (method == "POST" || method == "PUT")
            {
                string requestBody = request.Body ?? string.Empty;
                requestBody = requestBody.Replace("__Q__", "?").Replace("__D__", "&");

                // FIXED ENCTYPE: Changed fallback from x-www-form-urlencoded to application/octet-stream
                var contentType_ = request.Headers.ContainsKey("Content-Type") ? request.Headers["Content-Type"] : "application/octet-stream";

                forwardedRequest.Content = new StringContent(requestBody, Encoding.UTF8, contentType_);
            }
            forwardedRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("image/*"));
            var originalResponse = _client.Send(forwardedRequest);
            var responseBytes = originalResponse.Content.ReadAsByteArrayAsync().Result;
            byte[] Request_Bytes = null!;
            if (originalResponse.StatusCode == HttpStatusCode.NotModified && responseBytes.Length == 0)
            {
                Request_Bytes = _client.GetByteArrayAsync(originalUrl).Result;
            }
            HttpResponse httpResponse = new()
            {
                StatusCode = (int)originalResponse.StatusCode is 301 or 308 ? 202 : (int)originalResponse.StatusCode
            };

            var contentTypeHeader = originalResponse.Content.Headers.ContentType;
            var contentType = contentTypeHeader?.MediaType ?? "";

            if ((Request_Bytes is not null && responseBytes.Length is 0))
            {
                // BINARY CONTENT: Use SetBody(byte[], ...)
                httpResponse = new()
                {
                    StatusCode = 202
                };
                httpResponse.SetBody(Request_Bytes, "");

                if (httpResponse.Headers.ContainsKey("Content-Type"))
                    httpResponse.Headers.Remove("Content-Type");
            }
            else if (IsBinaryContent(contentType))
            {
                httpResponse.SetBody(responseBytes, contentType);
            }
            else
            {
                contentType ??= "application/octet-stream";
                // TEXT CONTENT: Convert to string
                string contentString = Encoding.UTF8.GetString(responseBytes);

                // Rewrite URLs if HTML, CSS, or JS
                if (IsHtmlOrCss(contentType))
                {
                    contentString = ModifyHtmlCssContent(config, contentString, contentType);
                }
                else if (IsJavaScript(contentType))
                {
                    contentString = ModifyJsContent(config, contentString);
                }

                // TEXT CONTENT: Use SetBody(string, ...)
                httpResponse.SetBody(contentString, $"{contentType}; charset=utf-8");
            }

            // Copy headers except Transfer-Encoding & Content-Length
            foreach (var header in originalResponse.Headers.Concat(originalResponse.Content.Headers))
            {
                var hKey = header.Key.ToLowerInvariant();
                if (hKey == "transfer-encoding" || hKey == "content-length")
                    continue;
                httpResponse.Headers[header.Key] = string.Join(", ", header.Value);
            }

            if (httpResponse.Headers.ContainsKey("Content-Security-Policy"))
            {
                var csp = httpResponse.Headers["Content-Security-Policy"];
                var modifiedCsp = csp
                    .Replace("default-src", "default-src blob: data: https: http: 'unsafe-inline' 'unsafe-eval'")
                    .Replace("style-src", "style-src 'unsafe-inline' http: https: data:")
                    .Replace("img-src", "img-src data: http: https: blob:")
                    .Replace("script-src", "script-src http: https: 'unsafe-inline' 'unsafe-eval'")
                    .Replace("media-src", "media-src http: https: blob: data:");

                httpResponse.Headers["Content-Security-Policy"] = modifiedCsp;
            }

            // After setting the body:
            if (httpResponse.Headers.ContainsKey("Content-Encoding"))
            {
                // The content is already decompressed by HttpClient, so remove this header
                httpResponse.Headers.Remove("Content-Encoding");
            }

            // Also remove Content-Length if you manually set it or 
            // if you've already called SetBody(...) which sets it correctly.
            if (httpResponse.Headers.ContainsKey("Content-Length"))
            {
                httpResponse.Headers.Remove("Content-Length");
            }

            if (!(Logrespon.IsFaulted || Logrespon.IsCanceled) && Logrespon.IsCompleted && Logrespon.Result is var logresp)
            {
                var r = config.Visitors.FirstOrDefault(x => x.RequestInformation.FirstOrDefault(x => x.Id == logresp.id) is not null)?.RequestInformation.FirstOrDefault(x => x.Id == logresp.id);
                if (r != null)
                {
                    r.Response = new RequestInformation()
                    {
                        RawBody = request.Body!,
                        Title = requestPath,
                        Headers = httpResponse.Headers,
                        StatusCode = httpResponse.StatusCode,
                        Timestamp = DateTime.Now,

                        Url = requestPath
                    };
                }
            }
            return httpResponse;
        }


        private bool IsBinaryContent(string contentType)
        {
            // Binary content: images, fonts, video, audio, or application/octet-stream
            if (contentType.StartsWith("image") ||
                contentType.StartsWith("video") ||
                contentType.StartsWith("audio") ||
                contentType.Contains("font") ||
                contentType == "application/octet-stream")
            {
                return true;
            }
            return false;
        }

        private bool IsHtmlOrCss(string contentType)
        {
            return contentType.Contains("text/html") || contentType.Contains("text/css");
        }

        private bool IsJavaScript(string contentType)
        {
            return contentType.Contains("javascript") || contentType.Contains("ecmascript");
        }

        //private string ModifyHtmlCssContent(Configuration config, string content, string contentType)
        //{
        //    var middlemanBase = $"/sv/{config.Id.ToString().Replace("-", "")}";

        //    if (contentType.Contains("html"))
        //    {
        //        // Rewrite HTML attributes: src, href, action, etc.
        //        string attrPattern = @"(?<attr>(src|href|action|data-src|data-href|poster))\s*=\s*(['""])(?<url>[^'""]+)\2";
        //        content = Regex.Replace(content, attrPattern, match =>
        //        {
        //            string attr = match.Groups["attr"].Value;
        //            string originalUrl = HttpUtility.HtmlDecode(match.Groups["url"].Value);
        //            string newUrl = RewriteSingleUrl(originalUrl, middlemanBase, config.Domain);
        //            return $"{attr}=\"{newUrl}\"";
        //        }, RegexOptions.IgnoreCase);

        //        // Inline CSS in HTML: url(...)
        //        string styleUrlPattern = @"url\((['""]?)(?<u>[^)]+)\1\)";
        //        content = Regex.Replace(content, styleUrlPattern, m =>
        //        {
        //            string cssUrl = HttpUtility.HtmlDecode(m.Groups["u"].Value);
        //            string newUrl = RewriteSingleUrl(cssUrl, middlemanBase, config.Domain);
        //            return $"url('{newUrl}')";
        //        }, RegexOptions.IgnoreCase);
        //    }
        //    else if (contentType.Contains("css"))
        //    {
        //        // In CSS files: rewrite url(...)
        //        string cssUrlPattern = @"url\((['""]?)(?<u>[^)]+)\1\)";
        //        content = Regex.Replace(content, cssUrlPattern, m =>
        //        {
        //            string cssUrl = HttpUtility.HtmlDecode(m.Groups["u"].Value);
        //            string newUrl = RewriteSingleUrl(cssUrl, middlemanBase, config.Domain);
        //            return $"url('{newUrl}')";
        //        }, RegexOptions.IgnoreCase);
        //    }

        //    return content;
        //}
        private string ModifyHtmlCssContent(Configuration config, string content, string contentType)
        {
            var middlemanBase = $"/sv/{config.Id.ToString().Replace("-", "")}";

            if (contentType.Contains("html"))
            {
                // Rewrite HTML attributes: src, href, action, etc.
                string attrPattern = @"(?<attr>(src|href|action|data-src|data-href|poster|data-bg|data-url))\s*=\s*(['""])(?<url>[^'""]+)\2";
                content = Regex.Replace(content, attrPattern, match =>
                {
                    string attr = match.Groups["attr"].Value;
                    string originalUrl = HttpUtility.HtmlDecode(match.Groups["url"].Value);
                    string newUrl = RewriteSingleUrl(originalUrl, middlemanBase, config.Domain);
                    return $"{attr}=\"{newUrl}\"";
                }, RegexOptions.IgnoreCase);

                // Inline CSS in HTML: url(...)
                string styleUrlPattern = @"url\((['""]?)(?<u>[^)]+)\1\)";
                content = Regex.Replace(content, styleUrlPattern, m =>
                {
                    string cssUrl = HttpUtility.HtmlDecode(m.Groups["u"].Value);
                    string newUrl = RewriteSingleUrl(cssUrl, middlemanBase, config.Domain);
                    return $"url('{newUrl}')";
                }, RegexOptions.IgnoreCase);

                // Meta tags: http-equiv refresh URLs
                string metaPattern = @"<meta\s+[^>]*http-equiv=['""]refresh['""]\s+content=['""]\d+;\s*url=(?<url>[^'""]+)['""]";
                content = Regex.Replace(content, metaPattern, match =>
                {
                    string originalUrl = HttpUtility.HtmlDecode(match.Groups["url"].Value);
                    string newUrl = RewriteSingleUrl(originalUrl, middlemanBase, config.Domain);
                    return match.Value.Replace(originalUrl, newUrl);
                }, RegexOptions.IgnoreCase);

                // Inline JavaScript URLs: location, fetch, XMLHttpRequest
                string jsUrlPattern = @"(location\s*=\s*|fetch\s*\(\s*['""]|XMLHttpRequest\(\s*['""])(?<url>[^'""]+)";
                content = Regex.Replace(content, jsUrlPattern, match =>
                {
                    string prefix = match.Groups[1].Value;
                    string originalUrl = HttpUtility.HtmlDecode(match.Groups["url"].Value);
                    string newUrl = RewriteSingleUrl(originalUrl, middlemanBase, config.Domain);
                    return $"{prefix}'{newUrl}'";
                }, RegexOptions.IgnoreCase);
            }
            else if (contentType.Contains("css"))
            {
                // In CSS files: rewrite url(...)
                string cssUrlPattern = @"url\((['""]?)(?<u>[^)]+)\1\)";
                content = Regex.Replace(content, cssUrlPattern, m =>
                {
                    string cssUrl = HttpUtility.HtmlDecode(m.Groups["u"].Value);
                    string newUrl = RewriteSingleUrl(cssUrl, middlemanBase, config.Domain);
                    return $"url('{newUrl}')";
                }, RegexOptions.IgnoreCase);
            }

            return content;
        }

        private string ModifyJsContent(Configuration config, string content)
        {
            // Rewrite URLs in JS strings
            if (string.IsNullOrEmpty(content)) return content; // Prevent processing empty content

            var middlemanBase = $"/sv/{config.Id.ToString().Replace("-", "")}";

            // **Fix 1:** Corrected regex and ensured safer capturing groups
            string jsUrlPattern = @"""((https?:\/\/[^\s'""<>]+|[a-zA-Z0-9\-]+\.[a-zA-Z]{2,})(\/[^\s'""<>]*)?)""";

            content = Regex.Replace(content, jsUrlPattern, match =>
            {
                string quote = match.Value[0].ToString();
                string originalUrl = match.Groups[1].Value; // Get full URL

                // **Fix 2:** Avoid rewriting already modified URLs
                if (originalUrl.StartsWith(middlemanBase) || originalUrl.Contains(config.Domain))
                    return match.Value;

                string newUrl = RewriteSingleUrl(originalUrl, middlemanBase, config.Domain);
                return $"{quote}{newUrl}{quote}";
            }, RegexOptions.IgnoreCase);

            // **Fix 3:** Corrected relative path replacement
            content = Regex.Replace(content, @"""\/([^""]+)""", match =>
            {
                string originalUrl = match.Groups[1].Value;
                string newUrl = $"{middlemanBase.TrimStart('/')}/{originalUrl}";

                return $"\"{newUrl}\""; // Ensuring correct formatting in JS
            }, RegexOptions.IgnoreCase);

            return content;
        }

        private string RewriteSingleUrl(string originalUrl, string middlemanBase, string upstreamDomain)
        {
            if (string.IsNullOrWhiteSpace(originalUrl))
                return originalUrl;

            if (originalUrl.StartsWith("data:") || originalUrl.StartsWith("#"))
            {
                // Don't rewrite data or anchor URLs
                return originalUrl;
            }

            string finalUrl = originalUrl;

            // If protocol-relative (e.g. //example.com)
            if (finalUrl.StartsWith("//"))
            {
                finalUrl = "http:" + finalUrl;
            }

            // If not starting with http, it's relative to upstream domain
            if (!finalUrl.StartsWith("http"))
            {
                string upstream = upstreamDomain.StartsWith("http") ? upstreamDomain : "http://" + upstreamDomain.TrimEnd('/');
                finalUrl = upstream.TrimEnd('/') + "/" + finalUrl.TrimStart('/');
            }

            // Replace ? and & with __Q__ and __D__
            finalUrl = finalUrl.Replace("?", "__Q__").Replace("&", "__D__");

            // Prepend proxy domain + middleman path
            finalUrl = $"{ProxyDomain}{middlemanBase}/{finalUrl}";

            // Clean potential duplicates
            finalUrl = finalUrl
                .Replace("///", "//")
                .Replace("http://http://", "http://")
                .Replace("https://https://", "https://");

            return finalUrl;
        }

        private string BuildUrl(string domain, string requestPath)
        {
            if (Regex.IsMatch(requestPath, @"^(?:[a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}(?:\/[^\s]*)?$"))
            {
                return requestPath.StartsWith("http") ? requestPath : "https://" + requestPath;
            }

            return $"{(domain.StartsWith("http") ? domain : "https://" + domain.TrimEnd('/'))}/{requestPath.TrimStart('/')}";
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
