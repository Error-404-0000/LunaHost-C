# 🌙✨ LunaHost - A Lightweight, Custom HTTP Server! or API or WEB any one works ✨

Hey there! Welcome to **LunaHost**—your super lightweight and powerful custom HTTP server in C#! LunaHost is here to make handling HTTP requests a breeze, with lots of cool features like flexible routing, middleware, and even Swagger UI for all your interactive docs! Think of it as a compact yet feature-rich alternative to ASP.NET!

## 🌸 Features You’ll Love:

### 💕 Attribute-Based Routing & Parameter Magic
LunaHost lets you define your routes easily using attributes! This makes setting up RESTful endpoints as simple as possible:
   - **HTTP Method Tags**: Define your endpoints like this: `[GetMethod]`, `[PostMethod]`, `[PutMethod]`, `[DeleteMethod]`.
   - **Parameter Magic**:
      - **`[FromRoute]`**: Grab a value from your URL.
      - **`[FromQuery]`**: Pick a value straight from the query string.
      - **`[FromHeader]`**: Snag a value from headers for some extra logic.
      - **`[FromBody]`**: Automatically turn that JSON request body into a usable object. How easy is that?!

### 🛠️ Middleware with Personalized Flair
LunaHost’s middleware system is super customizable!
   - **`ObjectPrefer` Attribute with `Preferred` Enum**: It lets middleware know exactly which parameters to handle. You can target values directly!
   - **`NoPreferences` Attribute**: Skip those unnecessary checks for routes that don’t need it—saving time.
   - **Dynamic Validation**: Middleware can check things like length, pattern, and whether the parameter is empty. Luna keeps everything organized and clean!

### 🌟 Fancy Validation with `Required`
Want to make sure your parameters are *just right*? Use the `Required` attribute to set rules:
   - **Length Limits**: Set minimum and maximum length.
   - **Regex Checks**: Match those strings to make sure they look exactly as you want.
   - **Not Null/Empty**: No empty names allowed here!

Example:
```csharp
[GetMethod("/users")]
public IHttpResponse GetUser([Required(3, 20, "Invalid username", new Regex("^[a-zA-Z]+$"))] string username)
{
    return HttpResponse.OK("User found");
}
```

### 📋 Swagger UI for Interactive Fun
Check out your APIs in a super fun and interactive way:
- Auto-Generated Docs: See all your routes turn into a pretty Swagger page!
- **Interactive Testing**: Yup, test right from your browser!
- **Full Details**: All headers, query strings, and body details—totally documented.

### 📈 Real-Time Logging and Monitoring
Need to keep track of what’s happening? LunaHost has built-in logging:
- **Centralized Logs**: Access them anytime at `/logs`.
- **Pagination**: Get them page by page if needed.

### 🛠️ Health Check? No Prob!
A health-check endpoint to make sure Luna’s all good to go:
- **Protected**: Access it using a unique build token at `/health/{Build_Token}/check`.

### 🧱 Advanced Routing
Create complex and dynamic routes like `/user/{username}/profile/{section}` for handling resources flexibly.

## 🏗️ Project Layout
```graphql
├── LunaHost
│   ├── Attributes                        # Routing and binding made easy!
│   ├── Enums                             # All those useful enums.
│   ├── Helper                            # Helpers like error pages and health-checks.
│   ├── HTTP                              # Core request/response handling.
│   ├── MiddleWares                       # Custom middleware to keep things functional.
│   ├── Swagger                           # All Swagger and OpenAPI magic.
├── LunaHostBuilder.cs                    # Builds the server. Simple!
├── Program.cs                            # Starts everything up.
└── README.md                             # You’re here! !
```

### 🔧 Setup Time!
1. **Clone the Repo**:
   ```bash
   git clone https://github.com/yourusername/LunaHost.git
   cd LunaHost
   ```
2. **Add Swagger UI**: 
   - Download Swagger UI from GitHub.
   - Place it in `Swagger/dist`. Update `swagger-initializer.js`.

3. **Run the Server**:
   - Open your fave IDE.
   - Run `Program.cs` and visit `http://localhost/docs`. 📚

## 🌟 Cute Examples:
### Capture Parameters with Validation
Want to capture both route and query parameters? Here’s how you do it, Luna-style!
```csharp
public class UserProfile : PageContent
{
    public UserProfile() : base("/user") {}

    [GetMethod("/{username}")]
    public IHttpResponse GetUserProfile(
        [FromRoute] string username, 
        [FromQuery] int age, 
        [Required(5, 20, "Invalid username")] string name)
    {
        return new HttpResponse
        {
            Body = $"User: {username}, Age: {age}, Name: {name}",
            StatusCode = 200
        };
    }
}
```

### NoPreferences for a Carefree Route
Don’t need middleware? Just tell Luna to skip it!
```csharp
[NoPreferences]
public class PublicEndpoints : PageContent
{
    [GetMethod("/public-info")]
    public IHttpResponse GetPublicInfo()
    {
        return HttpResponse.OK("This is public info. Enjoy!");
    }
}
```

### 🛣️ How Luna Works:
- **Routes Registration**: Loads all pages and routes.
- **Swagger Magic**: Generates `/swagger.json` automatically.
- **Middleware Execution**: Handles requests through middleware when needed.

### 📌 What’s Next for LunaHost?
- **Rate Limiting**: Keep the spammers out.
- **Role-Based Restrictions**: Keep it secure.
- **Caching**: Quick responses, always.

