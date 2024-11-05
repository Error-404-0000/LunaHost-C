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
    public sealed class SwaggerContent : PageContent
    {
        private readonly IEnumerable<PageContent> _pages;

        public SwaggerContent(IEnumerable<PageContent> pages) : base("/swagger.json")
        {
            _pages = pages ?? throw new ArgumentNullException(nameof(pages));
        }

        [GetMethod]
        public IHttpResponse GetSpec()
        {
            string openApiJson = GenerateOpenApiSpec();
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
            var paths = new Dictionary<string, object>();

            foreach (var page in _pages)
            {
                var methods = page.GetType().GetMethods();

                foreach (var method in methods)
                {
                    var httpMethodAttribute = method.GetCustomAttributes()
                        .OfType<IMethod>()
                        .FirstOrDefault();

                    if (httpMethodAttribute != null)
                    {
                        string path = httpMethodAttribute.Path; 
                        string httpMethod = httpMethodAttribute.GetType().Name.Replace("MethodAttribute", "").ToLower();

                        // Prepare parameters
                        var parameters = method.GetParameters()
                            .Select(param => new
                            {
                                tags = method.Name is string s && s.EndsWith("Content") && s.Length>7?CapitalizeFirstLetter(s.Remove(s.Length - 7)) : CapitalizeFirstLetter(method.Name),
                                name = param.Name,
                                @in = DetermineParameterLocation(param),
                                required = IsParameterRequired(param),
                                schema = new { type = GetSwaggerType(param.ParameterType) }
                            }).ToList();

                        // Add to OpenAPI path
                        if (!paths.ContainsKey(path))
                        {
                            paths[path] = new Dictionary<string, object>();
                        }

                        var pathMethods = (Dictionary<string, object>)paths[path];
                        pathMethods[httpMethod] = new
                        {
                            summary = $"Endpoint for {method.Name}",
                            parameters,
                            responses = new Dictionary<string, object>
                            {
                                { "200", new { description = "Successful response" } },
                                { "400", new { description = "Bad Request" } }
                            }
                        };
                    }
                }
            }

            var openApiSpec = new
            {
                openapi = "3.0.0",
                info = new
                {
                    title = "LunaHost API",
                    version = "1.0.0"
                },
                paths
            };

            return JsonConvert.SerializeObject(openApiSpec, Formatting.Indented);
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
