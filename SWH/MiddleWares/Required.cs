﻿using LunaHost.Attributes;
using LunaHost.Enums;
using LunaHost.Interfaces;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost;
using System.Text.RegularExpressions;
using LunaHost.Attributes.MiddleWares;

namespace LunaHost.MiddleWares;
[AttributeUsage(AttributeTargets.Parameter)]
[AsMiddleWare]
public class RequiredAttribute : Attribute, IMiddleWare
{
    private readonly int _minLen;
    private readonly int _maxLen;
    private readonly string _message;
    private readonly string? _regex;
    
    public RequiredAttribute(int minLen = -1, int maxLen = -1, string message = "Invalid input", string? regex = null)
    {
        _minLen = minLen;
        _maxLen = maxLen;
        _message = message;
        _regex = regex;
    }

    public async Task<IMiddleWareResult<IHttpResponse>> ExecuteAsync(HttpRequest request, [ObjectPrefer(Preferred.ParameterValue)] dynamic obj)
    {
        // Check for null or default
        if (obj is null or default(object) || obj is string s && (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s)))
        {
            return new MiddleWareResult<IHttpResponse>(HttpResponse.BadRequest(_message), false);
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

        // Check regex constraint
        if (_regex != null && obj is string regexObj && !Regex.IsMatch(regexObj,_regex))
        {
            return new MiddleWareResult<IHttpResponse>(HttpResponse.BadRequest($"{_message} - Does not match the required format"), false);
        }

        // If all checks pass, return OK
        return new MiddleWareResult<IHttpResponse>(HttpResponse.OK(), true);
    }
}
