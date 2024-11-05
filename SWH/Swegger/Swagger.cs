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
                paths.Add(
                     page.Path,
                      (
                       tags: page.GetType().Name,
                       path:page.Path
                      )
                    );
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
