using LunaHost;
using LunaHost.Attributes;
using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using MiddleWares;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
namespace LunaHost_Test
{
    internal class Program
    {
        public class MyCacheableObject : ICacheable
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int CacheCode { get; set; }

        }
        static void Main(string[] args)
        {

            while (true)
            {Cache<MyCacheableObject> cache = new Cache<MyCacheableObject>(20);

            }
            Console.ReadLine();
            #region
            //using (LunaHostBuilder Builder = new LunaHostBuilder(1))
            //{

            //    //returns full error
            //    Builder.InDebugMode = true;
            //    Builder.Add(new FirewallBlocked());
            //    Builder.Add(new Logger());
            //    Builder.Add(new UserApi());
            //    Builder.Add(new AccountContent());
            //    Builder.UseSwagger = true;
            //    Builder.BuildAsync().Wait();
            //}
            #endregion
        }
        #region Code
        public class AccountContent : PageContent
        {
            [PostMethod("/register")]
            public IHttpResponse Register([Required(5, 15, "Username is required", regex: "^[a-zA-Z0-9]*$"), FromBody] string username, [FromBody, Required] int id, [FromBody("bod")] HttpResponse r)
            {

                // Only called if the Required middleware validation passes
                return HttpResponse.OK(r.ToString());
            }
        }

        public class UserApi : PageContent
        {
            // Simulated in-memory storage for users and tokens
            private static Dictionary<string, string> _users = new Dictionary<string, string>();
            private static Dictionary<string, string> _tokens = new Dictionary<string, string>();

            public UserApi() : base("/api/user") { }

            [PostMethod("/login")]
            public IHttpResponse Login([FromBody] string username, [FromBody] string password)
            {
                if (_users.ContainsKey(username) && _users[username] == password)
                {
                    // Generate a token for the user
                    string token = Guid.NewGuid().ToString();
                    _tokens[username] = token;
                    return new HttpResponse
                    {
                        StatusCode = 200,
                        Body = $"Login successful! Token: {token}",
                        Headers = { ["Content-Type"] = "application/json" }
                    };
                }
                return HttpResponse.Unauthorized("Invalid username or password");
            }

            [PostMethod("/register")]
            public IHttpResponse Register([FromBody] string username, [FromBody] string password)
            {
                if (_users.ContainsKey(username))
                {
                    return HttpResponse.BadRequest("User already exists");
                }
                _users[username] = password;
                return HttpResponse.OK("User registered successfully");
            }

            [GetMethod("/getToken")]
            public IHttpResponse GetToken([FromHeader] string username)
            {
                if (_tokens.ContainsKey(username))
                {
                    return new HttpResponse
                    {
                        StatusCode = 200,
                        Body = $"Token for {username}: {_tokens[username]}",
                        Headers = { ["Content-Type"] = "application/json" }
                    };
                }
                return HttpResponse.NotFound("No token found for this user");
            }

            [PutMethod("/addUser")]
            public IHttpResponse AddUser([FromBody] string username, [FromBody] string password)
            {
                if (_users.ContainsKey(username))
                {
                    return HttpResponse.BadRequest("User already exists");
                }
                _users[username] = password;
                return HttpResponse.OK("User added successfully");
            }

            [PostMethod("/deleteUser")]
            public IHttpResponse DeleteUser([FromHeader] string username)
            {
                if (_users.ContainsKey(username))
                {
                    _users.Remove(username);
                    _tokens.Remove(username);
                    return HttpResponse.OK("User deleted successfully");
                }
                return HttpResponse.NotFound("User not found");
            }

            [GetMethod("/getAllUsers")]
            public IHttpResponse GetAllUsers()
            {
                string userList = string.Join(", ", _users.Keys);
                return new HttpResponse
                {
                    StatusCode = 200,
                    Body = $"Users: {userList}",
                    Headers = { ["Content-Type"] = "application/json" }
                };
            }

            [GetMethod("/verifyToken")]
            public IHttpResponse VerifyToken([FromHeader] string token)
            {
                if (_tokens.ContainsValue(token))
                {
                    return HttpResponse.OK("Token is valid");
                }
                return HttpResponse.Unauthorized("Invalid token");
            }

            [PutMethod("/updatePassword")]
            public IHttpResponse UpdatePassword([FromBody] string username, [FromBody] string newPassword)
            {
                if (_users.ContainsKey(username))
                {
                    _users[username] = newPassword;
                    return HttpResponse.OK("Password updated successfully");
                }
                return HttpResponse.NotFound("User not found");
            }
        }
    }
    public class FirewallBlocked : PageContent
    {
        public FirewallBlocked():base("/firewallblocked")
        {
            
        }
        [GetMethod("/blocked")]
        public IHttpResponse Get()
        {
            return HttpResponse.OK(@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Blocked by Firewall</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            font-family: Arial, sans-serif;
        }

        body {
            background-color: #f7f7f7;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
        }

        .container {
            background-color: #fff;
            border: 1px solid #ccc;
            border-radius: 10px;
            padding: 30px;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
            max-width: 600px;
            text-align: left;
        }

        .http-status {
            font-size: 24px;
            color: #d9534f;
            margin-bottom: 20px;
        }

        .header {
            font-size: 14px;
            color: #555;
            margin-bottom: 5px;
        }

        .message {
            margin-top: 20px;
            padding: 20px;
            background-color: #f2dede;
            border: 1px solid #ebccd1;
            border-radius: 5px;
        }

        .message h2 {
            color: #a94442;
            font-size: 22px;
            margin-bottom: 10px;
        }

        .message p {
            color: #a94442;
            font-size: 16px;
        }
    </style>
</head>
<body>
    <div class=""container"">
        <h1 class=""http-status"">HTTP/1.1 403 Forbidden</h1>
        <p class=""header"">Server:  </p>
        <p class=""header"">Date: Tue, 16 Oct 2024 10:30:00 GMT</p>
        <p class=""header"">Content-Type: text/html; charset=UTF-8</p>
        <p class=""header"">Content-Length: 636</p>
        <p class=""header"">Connection: close</p>

        <div class=""message"">
            <h2>Blocked by Firewall</h2>
            <p>Your request was blocked due to security rules. Please contact your administrator for more details.</p>
        </div>
    </div>
</body>
</html>
");
        }
    }
    public class Logger : PageContent
    {
        public Logger() : base("/logs")
        { }
        [GetMethod("/get-all-logs")]
        public IHttpResponse GetLogs()
        {
            var re = new HttpResponse()
            {
                Body = string.Join(",\n", LoggerAttribute.Loggers),
                StatusCode = 200,
            };
            re.Headers["Content-Type"] = "application/json";
            return re;
        }
        [GetMethod("/get-page")]
        public IHttpResponse GetTake([Required, FromUrlQuery("page-number")] int count)
        {
            var re = new HttpResponse()
            {
                Body = string.Join(",\n", LoggerAttribute.Loggers.Take(count)),
                StatusCode = 200,
            };
            re.Headers["Content-Type"] = "application/json";
            return re;
        }
        [GetMethod, Logger]
        public IHttpResponse Get()
        {
            return HttpResponse.OK();
        }

    }
    #endregion
}
