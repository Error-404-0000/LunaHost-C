using CacheLily.Attributes;
using LunaHost.Attributes;
using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
namespace LunaHost.Test
{
    public class UserApi : PageContent
    {
        private static Dictionary<string, string> _users = new();
        private static Dictionary<string, string> _tokens = new();

        public UserApi() : base("/api/user") { }

        [NoCaching]
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
        [NoCaching]
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
        [NoCaching]
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
        [NoCaching]
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
        [NoCaching]
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
