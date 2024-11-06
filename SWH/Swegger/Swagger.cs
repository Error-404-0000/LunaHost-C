using Attributes;
using HTTP.Interface;
using LunaHost.Attributes;
using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Swegger
{
    [NoPreferences]
    public sealed class SwaggerContent : PageContent
    {
        private readonly IEnumerable<PageContent> _pages;
        public record OpenApiSpec(List<Server> servers, Info info, string swagger_version);
        public record Info(string title, string version, string description);
        public record Server([JsonProperty("-url")] string url, string description);
        OpenApiSpec Swegger_info = null!;
        string openaiV = null!;
        string OpenAISpec_JSON = null;
        public SwaggerContent(IEnumerable<PageContent> pages, OpenApiSpec Info, string OpenaiVersion) : base("/swagger.json")
        {
            _pages = pages ?? throw new ArgumentNullException(nameof(pages));
            Swegger_info = Info ?? throw new ArgumentNullException(nameof(Info));
            openaiV = OpenaiVersion ?? throw new ArgumentNullException(nameof(OpenaiVersion));
        }


        [GetMethod]
        public IHttpResponse GetSpec()
        {
            string openApiJson = OpenAISpec_JSON ?? (OpenAISpec_JSON = GenerateOpenApiSpec());
            return new HttpResponse
            {
                Body = openApiJson,
                StatusCode = 200,
                Headers = { ["Content-Type"] = "application/json" }
            };
        }
        public static string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }

        public string GenerateOpenApiSpec()
        {
            var paths = new Dictionary<string, object>
    {
        { "openapi", "3.0.3" },
        { "info", Swegger_info.info },
        { "servers", Swegger_info.servers },
        { "tags", _pages.Select(x => new Dictionary<string, object> { { "name", x.Path } }).ToList() }
    };

            var page_info = new Dictionary<string, object>();

            foreach (var page in _pages)
            {
                var pageMethods = page.GetType()
                    .GetMethods()
                    .Where(m => m.GetCustomAttributes().Any(attr => attr is IMethod));

                foreach (var method in pageMethods)
                {
                    if (method.GetCustomAttributes().FirstOrDefault(attr => attr is IMethod) is IMethod im)
                    {
                        IHTTPParameter IHTTP = default(IHTTPParameter)!;
                        var parameters = method.GetParameters()
                            .Where(param => param.GetCustomAttributes().Any(attr => attr is IHTTPParameter IHTT && ((IHTTP = IHTT) is IHTTPParameter) && DetermineParameterLocation(param) != "body"))
                            .Select(param => new Dictionary<string, object>
                            {
                        { "name", (IHTTP is not null && IHTTP.IsSet) ? IHTTP.Name : param.Name ?? string.Empty },
                        { "in", DetermineParameterLocation(param) ?? "none" },
                        { "required", IsParameterRequired(param) },
                        { "schema", new Dictionary<string, object> { { "type", GetSwaggerType(param.ParameterType) } } }
                            })
                            .ToList();

                        var requestBodyProperties = method.GetParameters()
                            .Where(param => param.GetCustomAttributes().Any(attr => attr is IHTTPParameter IHTT && ((IHTTP = IHTT) is IHTTPParameter) && DetermineParameterLocation(param) == "body"))
                            .ToDictionary(param => (IHTTP is not null && IHTTP.IsSet) ? IHTTP.Name : param.Name ?? string.Empty, param => new Dictionary<string, object>
                            {
                        { "type", GetSwaggerType(param.ParameterType) },
                        { "default", GetSafeDefaultValue(param.ParameterType) }
                            });

                        var requestBody = requestBodyProperties.Any() ? new Dictionary<string, object>
                {
                    { "content", new Dictionary<string, object>
                        {
                            { "application/json", new Dictionary<string, object>
                                {
                                    { "schema", new Dictionary<string, object>
                                        {
                                            { "type", "object" },
                                            { "properties", requestBodyProperties }
                                        }
                                    }
                                }
                            }
                        }
                    }
                } : null;

                        var method_info = new Dictionary<string, object>
                {
                    { "tags", new object[] { page.Path } },
                    { "summary", "" },
                    { "parameters", parameters },
                    { "description", "" },
                    { "responses", new Dictionary<string, object>
                        {
                            { "200", new Dictionary<string, object> { { "description", "OK" } } }
                        }
                    }
                };

                        if (requestBody != null)
                        {
                            method_info["requestBody"] = requestBody;
                        }

                        string httpMethod = GetMethod(im);
                        string fullPath = page.Path + im.Path;

                        if (!page_info.ContainsKey(fullPath))
                        {
                            page_info[fullPath] = new Dictionary<string, object>();
                        }

                        ((Dictionary<string, object>)page_info[fullPath])[httpMethod] = method_info;
                    }
                }
            }

            paths["paths"] = page_info;

            return JsonConvert.SerializeObject(paths, Formatting.Indented);
        }
        private object GetSafeDefaultValue(Type type)
        {
            try
            {
                if (type == typeof(string)) return "";
                if (type == typeof(int) || type == typeof(long) || type == typeof(float) || type == typeof(double)) return 0;
                if (type == typeof(bool)) return false;
                if (type.IsClass) return Activator.CreateInstance(type)!;
                return type.IsValueType ? Activator.CreateInstance(type) !: default(object)!;
            }
            catch
            {
                return default(object)!; // Safe fallback in case instantiation fails
            }
        }



        private string GetMethod(IMethod imethod)
        {
            if (imethod == null)
                return "UNKNOWN";
            return imethod switch
            {
                GetMethodAttribute => "get",
                PostMethodAttribute => "post",
                PutMethodAttribute => "put",
                DeleteMethodAttribute => "delete",
                _ => "optional"
            };
        }
        private string DetermineParameterLocation(ParameterInfo param)
        {
            if (param.GetCustomAttribute<FromRoute>() != null) return "path";
            if (param.GetCustomAttribute<FromHeader>() != null) return "header";
            if (param.GetCustomAttribute<FromBody>() != null) return "body";
            if (param.GetCustomAttribute<FromUrlQuery>() != null) return "query";
            return "query"; // Default to query if location is unspecified
        }

        private bool IsParameterRequired(ParameterInfo param)
        {
            return param.GetCustomAttribute<RequiredAttribute>() != null;
        }

        private string GetSwaggerType(Type type)
        {
            // Map C# types to Swagger types
            return type == typeof(int) ? "integer" :
                   type == typeof(string) ? "string" :
                   type == typeof(bool) ? "boolean" :
                   type == typeof(double) ? "number" : "string";
        }
    }


}
