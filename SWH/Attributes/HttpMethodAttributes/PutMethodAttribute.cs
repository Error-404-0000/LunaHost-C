using LunaHost.HTTP.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LunaHost.Attributes.HttpMethodAttributes
{
    public class PutMethodAttribute: Attribute, IMethod
    {
        public string Path { get; set; } = "/";

        public UrlType UrlType { get; set; }
        public PutMethodAttribute(string urlPath, UrlType urlType = UrlType.Match)
        {
            Regex placeholderRegex = new Regex(@"{.*}");

            // Search for any placeholders in the URL path
            var matches = placeholderRegex.Matches(urlPath);

            // Check if there are any valid placeholders and invalid characters
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    // If there is at least one valid placeholder, set the UrlType to After
                    urlType = UrlType.After;
                }
            }

            // Set the Path property
            Path = urlPath;
            UrlType = urlType;
        }
        public PutMethodAttribute()
        {
            Path = "/";
            UrlType = UrlType.Match;

        }
    }
}
