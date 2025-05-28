# LunaHost - Lightweight Custom HTTP Server

**LunaHost** is a minimal and efficient custom HTTP server built in C#. It supports flexible routing, middleware, and includes optional Swagger UI for API documentation.


🐸##**Example Code :/LunaHost.Cache.Test**
## Features

### Attribute-Based Routing

Define routes using attributes to simplify RESTful API development:

* `[GetMethod]`, `[PostMethod]`, `[PutMethod]`, `[DeleteMethod]`
* Parameter binding with:

  * `[FromRoute]`: from the URL
  * `[FromQuery]`: from query string
  * `[FromHeader]`: from request headers
  * `[FromBody]`: from request body (JSON deserialization)

### Middleware System

* `ObjectPrefer` attribute and `Preferred` enum help target specific parameters.
* `NoPreferences` attribute disables middleware for certain routes.
* Dynamic validation: check length, pattern, and empty values.

### Parameter Validation

Use the `Required` attribute to enforce:

* Length limits
* Regex pattern match
* Null or empty check

Example:

```csharp
[GetMethod("/users")]
public IHttpResponse GetUser([Required(3, 20, "Invalid username", new Regex("^[a-zA-Z]+$"))] string username)
{
    return HttpResponse.OK("User found");
}
```

### Swagger UI (Optional)

* Automatically generated API documentation.
* Supports browser-based API testing.
* Displays complete request/response data.

### Logging and Monitoring

* Access logs at `/logs`
* Supports pagination

### Health Check

* Endpoint at `/health/{Build_Token}/check`
* Requires a build token

### Advanced Routing

Supports complex routes like `/user/{username}/profile/{section}`

## Project Structure

```
├── LunaHost
│   ├── Attributes
│   ├── Enums
│   ├── Helper
│   ├── HTTP
│   ├── MiddleWares
│   ├── Swagger
├── LunaHostBuilder.cs
└── README.md
```

## Setup

1. Clone the repository:

```bash
git clone https://github.com/Error-404-0000/LunaHost.git
cd LunaHost
```

2. Enable Swagger UI if needed:

```csharp
UseSwagger = true;
```

## Usage Examples

### Capture Route and Query Parameters

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

### Disable Middleware

```csharp
[NoPreferences]
public class PublicEndpoints : PageContent
{
    [GetMethod("/public-info")]
    public IHttpResponse GetPublicInfo()
    {
        return HttpResponse.OK("This is public info.");
    }
}
```

### Internal Functionality

* Registers routes and pages.
* Auto-generates `/swagger.json`.
* Executes middleware logic where applicable.

## Planned Features

* Rate limiting
* Role-based access control
* Response caching
