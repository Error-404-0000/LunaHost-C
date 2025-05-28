using LunaHost.Attributes;
using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost.MiddleWares;

namespace LunaHost.Test
{
    [_LoggerAttribute]//Logs every request sent to anyobject within the Class
    public class UserRoute : PageContent
    {
        public static readonly List<UserModel> UserDb = new List<UserModel>();

        public UserRoute() : base("/users") { }


        [PostMethod("/create/{username}")]
        public IHttpResponse CreateUser(
            [FromBody,CantBeNull] UserModel body)
        {
            UserDb.Add(new(body.Email, body.Password, body.Age));
            return HttpResponse.OK($"User {body.Email} created");
        }

        [PostMethod("/login")]
        public IHttpResponse Login(
            [FromBody] UserModel user)
        {
            var exists = UserDb.Any(u => u.Email == user.Email && u.Password == user.Password);
            return exists ? HttpResponse.OK("Login success") : HttpResponse.BadRequest("Invalid credentials");
        }

        [PostMethod("/edit")]
        public IHttpResponse EditUser(
            [FromBody][CantBeNull] UserModel user,
            [FromUrlQuery] int age)
        {
            for (int i = 0; i < UserDb.Count; i++)
            {
                if (UserDb[i].Email == user.Email)
                {
                    UserDb[i] = new(user.Email, user.Password, age);
                    return HttpResponse.OK("User updated");
                }
            }
            return HttpResponse.BadRequest("User not found");
        }

        [GetMethod("/profile/{Email}")]
        public IHttpResponse GetProfile(
            [FromRoute,
            CantBeNull,
            Required(maxLen:50,message:"invalid Email Address",regex: @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")] string Email)
        {
            var user = UserDb.FirstOrDefault(u => u.Email == Email);
            return user?.Email != null
                ? HttpResponse.OK($"Username: {user.Email}, Age: {user.Age}")
                : HttpResponse.BadRequest("User not found");
        }

        [GetMethod("/validate")]
        public IHttpResponse ValidateUser(
            [Required(3, 20, "Bad name", "^[a-zA-Z]+$")] string name)
        {
            return HttpResponse.OK("Valid!");
        }
    }

}
