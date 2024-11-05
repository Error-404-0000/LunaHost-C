
# LunaHost 🌕 - A Custom HTTP Server with Advanced Routing, Middleware, and Swagger Integration

Welcome to **LunaHost**—a custom-built HTTP server in C# that’s highly modular and feature-rich! LunaHost is designed to be a flexible playground for handling HTTP requests, implementing middleware, and generating interactive documentation with Swagger UI. This project dives deep into custom routing, parameter validation, middleware management, and seamless API documentation.

## 🚀 Features

### 🧩 Attribute-Based Routing & Parameter Binding
LunaHost enables declarative route definitions using attributes. This feature allows you to create RESTful endpoints easily and bind request data directly to method parameters:
   - **HTTP Method Attributes**: Define endpoints using `[GetMethod]`, `[PostMethod]`, `[PutMethod]`, and `[DeleteMethod]`.
   - **Parameter Binding Attributes**:
      - **`[FromRoute]`**: Bind URL route segments to method parameters.
      - **`[FromQuery]`**: Capture query string values directly.
      - **`[FromHeader]`**: Bind HTTP headers to parameters for custom header-based logic.
      - **`[FromBody]`**: Automatically deserialize request bodies (e.g., JSON) into method parameters.

### ⚙️ Middleware System with Preference Control
LunaHost’s middleware system is highly customizable, supporting parameter-based control over middleware execution:
   - **`ObjectPrefer` Attribute with `Preferred` Enum**: Allows middleware to specify which parameters they want to handle directly. This can be used to target parameters by `Preferred.ParameterValue` or other custom preferences, helping middleware determine the exact parameters it should process.
   - **`NoPreferences` Attribute**: Use `NoPreferences` to bypass certain middleware checks, optimizing routes that don’t need all middleware layers.
   - **Dynamic Validation Middleware**: Middleware can enforce rules, such as min/max length, regex matching, and null/empty checks.

### 🧪 Parameter Validation with `Required` Attribute
The `Required` attribute acts as a powerful validation middleware, enabling automatic checks on parameters. You can enforce:
   - **Length Constraints**: Specify minimum and maximum lengths.
   - **Regex Patterns**: Match input against regular expressions for custom formats.
   - **Null/Empty Checks**: Prevent parameters from being null or empty by default.

Example:
```csharp
[GetMethod("/users")]
public IHttpResponse GetUser([Required(3, 20, "Invalid username", new Regex("^[a-zA-Z]+$"))] string username)
{
    return HttpResponse.OK("User found");
}
```
###📋 Swagger UI for API Documentation
LunaHost generates an OpenAPI specification on the fly, allowing you to view and test your API endpoints interactively at /docs:

Auto-Generated OpenAPI Spec: Reflects over your route definitions to produce swagger.json.
Interactive Testing: Test endpoints directly from the browser.
Complete Parameter Documentation: View all required headers, query strings, route parameters, and request body details.
📈 Real-Time Logging and Monitoring
Built-in logging functionality lets you access logs at any time. The Logger middleware and page enable endpoint-level logging:

Centralized Logging: View logs by accessing /logs.
Pagination Support: Retrieve logs in paginated form using query parameters.
🛠️ Health Check Endpoint
LunaHost includes a health-check endpoint to verify server readiness:

Protected Endpoint: Accessed via a unique build token at /health/{Build_Token}/check.
Customizable Checks: Configure health-checks to meet your server’s readiness needs.
🧱 Advanced Routing Capabilities
LunaHost supports complex routes, such as /user/{username}/profile/{section}, allowing you to capture dynamic segments using FromRoute attributes. This makes it ideal for RESTful API design with nested resources and custom routes.

🏗️ Project Structure
```graphql
Copy code
├── LunaHost
│   ├── Attributes                        # Attributes for routing and parameter binding
│   │   ├── HttpMethodAttributes          # HTTP method attributes (GET, POST, etc.)
│   │   ├── Middleware                    # Middleware preference attributes
│   ├── Enums                             # Enums for HTTP methods and middleware preferences
│   ├── Helper                            # Helper classes like error and health-check pages
│   ├── HTTP                              # Core HTTP request/response handling
│   ├── MiddleWares                       # Custom middleware classes
│   ├── Swagger                           # Swagger UI and OpenAPI generation
├── LunaHostBuilder.cs                    # Server builder and main configuration
├── Program.cs                            # Initializes server and adds routes
└── README.md                             # Project documentation (you’re here!)
🔧 Installation & Setup
Clone the Repository:
```
```bash
Copy code
git clone https://github.com/yourusername/LunaHost.git
cd LunaHost
```
### Add Swagger UI Files:

Download the Swagger UI dist folder from Swagger UI GitHub.
Place it in the Swagger/dist directory. Update swagger-initializer.js to use url: "/swagger.json".
Run the Server:

**Open the project in Visual Studio or your preferred C# IDE.
Run Program.cs to start the server.
Access Swagger UI:**
```docs
Open your browser and navigate to http://localhost/docs for interactive documentation.
The OpenAPI specification is available at http://localhost/swagger.json.
```
# 📚 Examples
Define an Endpoint with Parameter Binding and Validation
Here’s an example of an endpoint that captures both route and query parameters, with validation middleware applied.

```csharp
Copy code
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
Use ObjectPrefer for Middleware Control
This example uses ObjectPrefer to specify which parameter the middleware should target directly, helping middleware determine which specific parameter to handle.

csharp
Copy code
public class CriticalEndpoints : PageContent
{
    [GetMethod("/important")]
    public IHttpResponse ImportantAction([ObjectPrefer(Preferred.ParameterValue), Required(1, 100)] int criticalValue)
    {
        return HttpResponse.OK("Critical action executed");
    }
}
Apply NoPreferences to Bypass Middleware
Use the NoPreferences attribute to skip certain middleware checks for routes or classes that don’t require them.

csharp
Copy code
[NoPreferences]
public class PublicEndpoints : PageContent
{
    [GetMethod("/public-info")]
    public IHttpResponse GetPublicInfo()
    {
        return HttpResponse.OK("This is public information, no preferences needed.");
    }
}
🛣️ How LunaHost Works
Route Registration: Loads all **PageContent** classes, applying HTTP method attributes to set up routes.
OpenAPI Spec Generation: Reflects over methods to generate an OpenAPI JSON spec (/swagger.json).
Middleware Execution: Processes requests through middleware with 
```python
[ObjectPrefer(Preferred.Any)]
[NoPreference]
```
###and NoPreferences controls.
###📌 Future Enhancements
###LunaHost is a foundation for server concepts. Possible future upgrades:

Rate Limiting: Throttle requests to prevent overuse.
Role-Based Access Control: Add role-based restrictions with middleware.
Caching: Implement caching for frequently accessed resources.
"""

