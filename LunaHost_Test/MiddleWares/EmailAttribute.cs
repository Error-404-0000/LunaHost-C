using LunaHost.Attributes;
using LunaHost.Enums;
using LunaHost.Interfaces;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost;
using System.Text.RegularExpressions;
using LunaHost.Attributes.MiddleWares;

namespace LunaHost.MiddleWares
{
    [AttributeUsage(AttributeTargets.Parameter)]
    [AsMiddleWare]
    public class EmailAttribute : Attribute, IMiddleWare
    {
        private readonly int _minLen;
        private readonly int _maxLen;
        private readonly string _message;

        // Regular expression for validating email format
        private const string EmailRegex = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";

        public EmailAttribute(int minLen = -1, int maxLen = -1, string message = "Invalid email address")
        {
            _minLen = minLen;
            _maxLen = maxLen;
            _message = message;
        }

        public async Task<IMiddleWareResult<IHttpResponse>> ExecuteAsync(HttpRequest request, [ObjectPrefer(Preferred.ParameterValue)] dynamic obj)
        {
            // Check if the email is null or empty
            if (obj is null or default(object) || obj is string s && string.IsNullOrWhiteSpace(s))
            {
                return new MiddleWareResult<IHttpResponse>(HttpResponse.BadRequest($"{_message} - Email cannot be empty"), false);
            }

            // Check for minimum length constraint
            if (obj is string strObj && _minLen >= 0 && strObj.Length < _minLen)
            {
                return new MiddleWareResult<IHttpResponse>(HttpResponse.BadRequest($"{_message} - Minimum length is {_minLen}"), false);
            }

            // Check for maximum length constraint
            if (obj is string maxObj && _maxLen >= 0 && maxObj.Length > _maxLen)
            {
                return new MiddleWareResult<IHttpResponse>(HttpResponse.BadRequest($"{_message} - Maximum length is {_maxLen}"), false);
            }

            // Validate email format using regex
            if (obj is string email && !Regex.IsMatch(email, EmailRegex))
            {
                return new MiddleWareResult<IHttpResponse>(HttpResponse.BadRequest($"{_message} - Invalid email format"), false);
            }

            // If all checks pass, return OK
            return new MiddleWareResult<IHttpResponse>(HttpResponse.OK(), true);
        }
    }
}
