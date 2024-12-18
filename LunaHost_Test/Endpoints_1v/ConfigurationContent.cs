using LunaHost.Attributes;
using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost_Test.Db;
using LunaHost_Test.Routes;
using LunaHost_Test.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LunaHost_Test.Endpoints_1v
{
    [Route("/1v/config")]
    public class ConfigurationContent : PageContent
    {
        /// <summary>
        /// Retrieves all configurations for the authenticated user.
        /// </summary>
        /// <param name="token">The authorization token.</param>
        [GetMethod]
        public IHttpResponse GetAllConfigs([FromHeader("token")] string token)
        {
            // Validate token and find the user
            if (AppDb.Users.FirstOrDefault(u => u.Tokens.Any(t => t.UserToken == token && t.IsTokenAlive)) is User user)
            {
                var configurations = AppDb.Configurations
                    .Where(x => x.UserId == user.Id)
                    .ToList();

                if (!configurations.Any())
                {
                    return HttpResponse.NotFound("No configurations found for this user.");
                }

                return HttpResponse.OK(JsonConvert.SerializeObject(configurations));
            }

            return HttpResponse.Unauthorized("Invalid token.");
        }

        /// <summary>
        /// Searches configurations based on filters like name, domain, etc.
        /// </summary>
        /// <param name="token">The authorization token.</param>
        /// <param name="searchTerm">Search term to filter configurations.</param>
        /// <returns>Filtered configurations.</returns>
        [GetMethod("/search")]
        public IHttpResponse SearchConfigs(
            [FromHeader("token")] string token,
            [FromUrlQuery("term")] string searchTerm)
        {
            if (AppDb.Users.FirstOrDefault(u => u.Tokens.Any(t => t.UserToken == token && t.IsTokenAlive)) is User user)
            {
                var filteredConfigs = AppDb.Configurations
                    .Where(x => x.UserId == user.Id &&
                                (x.ConfigurationName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                 x.Domain.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (!filteredConfigs.Any())
                {
                    return HttpResponse.NotFound($"No configurations matching the search term '{searchTerm}' were found.");
                }

                return HttpResponse.OK(JsonConvert.SerializeObject(filteredConfigs));
            }

            return HttpResponse.Unauthorized("Invalid token.");
        }

        /// <summary>
        /// Retrieves a specific configuration by ID.
        /// </summary>
        [GetMethod("/{id}")]
        public IHttpResponse GetConfig([FromRoute] string id, [FromHeader("token")] string token)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpResponse.BadRequest($"Invalid ID provided. (Path: {this.Path})");
            }

            if (AppDb.Users.FirstOrDefault(u => u.Tokens.Any(t => t.UserToken == token && t.IsTokenAlive)) is User user)
            {
                var configuration = AppDb.Configurations.FirstOrDefault(x => x.Id.ToString() == id && x.UserId == user.Id);

                if (configuration == null)
                {
                    return HttpResponse.NotFound("Configuration not found.");
                }

                return HttpResponse.OK(JsonConvert.SerializeObject(configuration));
            }

            return HttpResponse.Unauthorized("Invalid token.");
        }

        /// <summary>
        /// Updates a specific configuration by ID.
        /// </summary>
        [PutMethod("/{id}")]
        public IHttpResponse UpdateConfig(
            [FromRoute] string id,
            [FromHeader("token")] string token,
            Configuration updatedConfig)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpResponse.BadRequest($"Invalid ID provided. (Path: {this.Path})");
            }

            if (AppDb.Users.FirstOrDefault(u => u.Tokens.Any(t => t.UserToken == token && t.IsTokenAlive)) is User user)
            {
                var configuration = AppDb.Configurations.FirstOrDefault(x => x.Id.ToString() == id && x.UserId == user.Id);

                if (configuration == null)
                {
                    return HttpResponse.NotFound("Configuration not found.");
                }
                if (AppDb.Configurations.Any(x => (x.ConfigurationName == updatedConfig.ConfigurationName&&configuration.ConfigurationName!=updatedConfig.ConfigurationName) && 
                user.Id == x.Id))
                {
                    return HttpResponse.BadRequest("Name already exist in Config Man.");
                }
                // Apply updates
                configuration.ConfigurationName = updatedConfig.ConfigurationName ?? configuration.ConfigurationName;
                configuration.Domain = updatedConfig.Domain ?? configuration.Domain;
                configuration.StartPath = updatedConfig.StartPath ?? configuration.StartPath;
                configuration.EnableLogging = updatedConfig.EnableLogging;

                return HttpResponse.OK("Configuration updated successfully.");
            }

            return HttpResponse.Unauthorized("Invalid token.");
        }

        /// <summary>
        /// Deletes a specific configuration by ID.
        /// </summary>
        [DeleteMethod("/{id}")]
        public IHttpResponse DeleteConfig([FromRoute] string id, [FromHeader("token")] string token)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpResponse.BadRequest($"Invalid ID provided. (Path: {this.Path})");
            }

            if (AppDb.Users.FirstOrDefault(u => u.Tokens.Any(t => t.UserToken == token && t.IsTokenAlive)) is User user)
            {
                var configuration = AppDb.Configurations.FirstOrDefault(x => x.Id.ToString() == id && x.UserId == user.Id);

                if (configuration == null)
                {
                    return HttpResponse.NotFound("Configuration not found.");
                }

                AppDb.Configurations.Remove(configuration);

                return HttpResponse.OK("Configuration deleted successfully.");
            }

            return HttpResponse.Unauthorized("Invalid token.");
        }

        /// <summary>
        /// Creates a new configuration for the authenticated user.
        /// </summary>
        [PostMethod]
        public IHttpResponse CreateConfig([FromHeader("token")] string token,[FromBody] Configuration newConfig)
        {
            if (AppDb.Users.FirstOrDefault(u => u.Tokens.Any(t => t.UserToken == token && t.IsTokenAlive)) is User user)
            {
                newConfig.Visitors = new();
                newConfig.UserId = user.Id; // Link configuration to the user
                if(AppDb.Configurations.Any(x=>x.ConfigurationName==newConfig.ConfigurationName&& user.Id == x.UserId))
                {
                    return HttpResponse.BadRequest("Name already exist in Config Man.");
                }
                AppDb.Configurations.Add(newConfig);

                return HttpResponse.OK(JsonConvert.SerializeObject(new
                {
                    Message = "Configuration created successfully.",
                    ConfigId = newConfig.Id.ToString().Replace("-","")
                }));
            }

            return HttpResponse.Unauthorized("Invalid token.");
        }
    }
}
