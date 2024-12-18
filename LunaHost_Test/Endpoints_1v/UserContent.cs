using CacheLily.Attributes;
using LunaHost.Attributes;
using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost.MiddleWares;
using LunaHost_Test.Db;
using LunaHost_Test.Users;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost_Test.Endpoints_1v
{
    /// <summary>
    /// User management endpoints for handling user actions like registration, authentication, and updates.
    /// </summary>
    [Route("/1v/user")]
    [NoCaching] // Prevents caching for dynamic user data
    [LunaHost.MiddleWares.Security.DDoSProtection.Redirector] // Middleware to prevent DDoS attacks
    public class UserContent : PageContent
    {
        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="Email">The email address of the user.</param>
        /// <param name="UserName">The username of the user.</param>
        /// <param name="password">The password for the user account.</param>
        /// <returns>An HTTP response indicating success or failure.</returns>
        [PostMethod("/register")]
        public IHttpResponse Register(
            [FromBody("email"), Email] string Email,
            [FromBody("username"), Required(minLen: 5, maxLen: 10,message:"invalid username", regex: @"\w+")] string UserName,
            [Required(minLen: 6,message:"invalid password", regex: @".+"),FromBody] string password)
        {
            // Check if a user with the same email already exists
            if (AppDb.Users.Any(x => x.Email == Email))
            {
                return HttpResponse.BadRequest("User already exists");
            }

            // Create and add a new user object
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = Email,
                UserName = UserName,
                PasswordHash = HashPassword(password), // Hash the password before storing
                JoinedDate = DateTime.UtcNow
            };

            AppDb.Users.Add(newUser); // Add user to the in-memory database

            // Return a success response
            return HttpResponse.OK("User registered successfully" );
        }

        /// <summary>
        /// Retrieves user information based on the authorization and UserToken headers.
        /// </summary>
        /// <param name="auth">Authorization UserToken from the header.</param>
        /// <param name="token">Session UserToken from the header.</param>
        /// <returns>User details if authentication is successful; otherwise, an Unauthorized response.</returns>
        [RequiredHeader("Authorization")]
        [GetMethod("/user-info")]
        public IHttpResponse GetUserInformation(
            [FromHeader("Authorization")] string auth,
            [FromHeader("token")] string token)
        {
            // Find user by authorization and UserToken
            if (AppDb.Users.FirstOrDefault(x => x.UserAuth == auth && x.Tokens.Any(t => t.UserToken == token && t.IsTokenAlive)) is User user)
            {
                return HttpResponse.OK(JsonConvert.SerializeObject(new
                {
                    UserId = user.Id,
                    user.JoinedDate,
                    user.UserName,
                    user.TotalDomain,
                    user.Email,
                    user.LastActive,
                    ConfigurationAction = JsonConvert.SerializeObject(AppDb.Configurations.FirstOrDefault(x => x.UserId == user.Id))
                }));
            }

            // Return Unauthorized if no matching user is found
            return HttpResponse.Unauthorized();
        }

        /// <summary>
        /// Changes the password of an authenticated user and invalidates all tokens.
        /// </summary>
        /// <param name="auth">Authorization UserToken from the header.</param>
        /// <param name="newpassword">The new password to set for the user.</param>
        /// <returns>Success response if the operation completes; otherwise, Unauthorized.</returns>
        [RequiredHeader("Authorization")]
        [PutMethod("/change-password")]
        public IHttpResponse ChangePassword(
            [FromHeader("Authorization")] string auth,
            [Required(minLen: 6, message: "invalid password", regex: @".+"), FromBody] string newpassword)
        {
            // Find the user by authorization UserToken
            var user = AppDb.Users.FirstOrDefault(x => x.UserAuth == auth);
            if (user is null) return HttpResponse.Unauthorized();

            // Update the password and invalidate tokens
            user.PasswordHash = HashPassword(newpassword);
            user.Tokens.Clear(); // Remove all tokens after a password change

            return HttpResponse.OK("Password updated successfully");
        }

        /// <summary>
        /// Generates a new session UserToken for the user based on username and password.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>A new session UserToken and authorization if authentication is successful.</returns>
        [PostMethod("/new-section")]
        public IHttpResponse Get_New_Token(
            [FromBody] string username,
            [FromBody] string password)
        {
            // Hash the provided password
            var hashedPassword = HashPassword(password);

            // Authenticate user by username and hashed password
            if (AppDb.Users.FirstOrDefault(x => x.UserName == username && x.PasswordHash == hashedPassword) is User user)
            {
                var newToken = new Token
                (
                  GenerateToken(),
                  DateTime.UtcNow.AddDays(30)
                );

                user.Tokens.Add(newToken); // Add the new UserToken to the user's UserToken list

                return HttpResponse.OK(JsonConvert.SerializeObject( new
                {
                    Token = newToken.UserToken,
                    Authorization = user.UserAuth
                }));
            }

            // Return Unauthorized if authentication fails
            return HttpResponse.Unauthorized();
        }

        /// <summary>
        /// Hashes a password using SHA256.
        /// </summary>
        /// <param name="password">The plain text password to hash.</param>
        /// <returns>The hashed password as a Base64 string.</returns>
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Generates a new unique UserToken.
        /// </summary>
        /// <returns>A newly generated UserToken string.</returns>
        private string GenerateToken()
        {
            return Guid.NewGuid().ToString("N"); // Generate a 32-character UserToken
        }
    }
}
