# HTTP API Extensions for Azure Functions

[![Build](https://github.com/shibayan/azure-functions-http-api/workflows/Build/badge.svg)](https://github.com/shibayan/azure-functions-http-api/actions/workflows/build.yml)
[![License](https://badgen.net/github/license/shibayan/azure-functions-http-api)](https://github.com/shibayan/azure-functions-http-api/blob/master/LICENSE)

| Package | NuGet |
|---------|-------|
| WebJobs.Extensions.HttpApi (In-Process) | [![NuGet](https://badgen.net/nuget/v/WebJobs.Extensions.HttpApi)](https://www.nuget.org/packages/WebJobs.Extensions.HttpApi/) [![Downloads](https://badgen.net/nuget/dt/WebJobs.Extensions.HttpApi)](https://www.nuget.org/packages/WebJobs.Extensions.HttpApi/) |
| Functions.Worker.Extensions.HttpApi (Isolated Worker) | [![NuGet](https://badgen.net/nuget/v/Functions.Worker.Extensions.HttpApi)](https://www.nuget.org/packages/Functions.Worker.Extensions.HttpApi/) [![Downloads](https://badgen.net/nuget/dt/Functions.Worker.Extensions.HttpApi)](https://www.nuget.org/packages/Functions.Worker.Extensions.HttpApi/) |

An extension library that brings ASP.NET Core-like developer experience to Azure Functions. Inherit from `HttpFunctionBase` to get access to familiar helpers such as `Ok()`, `BadRequest()`, `File()`, model validation, URL generation, and more — in both **In-Process** and **Isolated Worker** models.

## Features

- **Model validation** — Validate request models with DataAnnotations using `TryValidateModel()` and return `BadRequest(ModelState)` or `ValidationProblem(ModelState)`
- **ASP.NET Core-like helpers** — Use familiar methods such as `Ok()`, `BadRequest()`, `NotFound()`, `Unauthorized()`, `Content()`, `File()`, and access `Request` / `Response` / `User` directly
- **URL generation** — Generate URLs to other functions with `CreatedAtFunction()` and `AcceptedAtFunction()`
- **Static file hosting** — Serve files from `wwwroot` with automatic content-type detection
- **Reverse proxy** — Forward requests to backend services with `Proxy()`, supporting route template substitution
- **SPA / SSG hosting** — Host single-page applications with `LocalStaticApp()`, including fallback routing and path exclusion
- **App Service Authentication** — Access authenticated user info via the `User` property with App Service Authentication (EasyAuth) support
- **Better route precedence** — Improved route matching behavior over default Azure Functions routing

## Installation

### .NET Isolated Worker (Recommended)

```
dotnet add package Functions.Worker.Extensions.HttpApi
```

### .NET In-Process

```
dotnet add package WebJobs.Extensions.HttpApi
```

## Quick Start

### Isolated Worker

Use ASP.NET Core integration in `Program.cs`. The extension is registered automatically by the package.

```csharp
// Program.cs
var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Build().Run();
```

```csharp
// Function1.cs
public class Function1(IHttpContextAccessor httpContextAccessor) : HttpFunctionBase(httpContextAccessor)
{
    [Function("Function1")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        return Ok($"Hello, {req.Query["name"]}");
    }
}
```

### In-Process

```csharp
public class Function1(IHttpContextAccessor httpContextAccessor) : HttpFunctionBase(httpContextAccessor)
{
    [FunctionName("Function1")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        return Ok($"Hello, {req.Query["name"]}");
    }
}
```

## Examples

### Model Validation

Use `TryValidateModel()` with DataAnnotations to validate incoming request bodies.

```csharp
public class Function1(IHttpContextAccessor httpContextAccessor) : HttpFunctionBase(httpContextAccessor)
{
    [FunctionName("Function1")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] SampleModel model)
    {
        if (!TryValidateModel(model))
        {
            return BadRequest(ModelState);
        }

        return Ok(model);
    }
}

public class SampleModel
{
    [Required]
    public string Name { get; set; }

    public string[] Array { get; set; }

    [Range(100, 10000)]
    public int Price { get; set; }
}
```

You can also return RFC 7807 Problem Details format using `ValidationProblem()`:

```csharp
if (!TryValidateModel(model))
{
    return ValidationProblem(ModelState);
}
```

### Accessing Request / Response / User

Access `HttpContext`, `Request`, `Response`, and `User` properties directly, just like in ASP.NET Core controllers.

```csharp
public class Function2(IHttpContextAccessor httpContextAccessor) : HttpFunctionBase(httpContextAccessor)
{
    [FunctionName("Function2")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        Response.Headers.Add("Cache-Control", "no-cache");

        return Ok($"Now: {DateTime.Now}");
    }
}
```

### URL Generation

Generate URLs to other functions using `CreatedAtFunction()` and `AcceptedAtFunction()`.

```csharp
public class Function3(IHttpContextAccessor httpContextAccessor) : HttpFunctionBase(httpContextAccessor)
{
    [FunctionName("Function3")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "route/{id}")] HttpRequest req,
        string id)
    {
        return CreatedAtFunction("Function3", new { id = "kazuakix" }, null);
    }
}
```

### Static File Hosting

Serve files from the `wwwroot` directory with automatic content-type detection.

```csharp
public class Function1(IHttpContextAccessor httpContextAccessor) : HttpFunctionBase(httpContextAccessor)
{
    [FunctionName("Function1")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        return File("sample.html");
    }
}
```

### Reverse Proxy

Forward incoming requests to a backend service. Route parameters are automatically substituted in the backend URI.
You can also inspect or modify the proxied request and response with async hooks.

```csharp
public class ReverseProxy(IHttpContextAccessor httpContextAccessor) : HttpFunctionBase(httpContextAccessor)
{
    [FunctionName(nameof(ReverseProxy))]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "{*path}")] HttpRequest req)
    {
        return Proxy(
            "https://example.com/{path}",
            async request =>
            {
                request.Headers.Add("X-Forwarded-By", "azure-functions-http-api");
                await Task.CompletedTask;
            },
            async response =>
            {
                response.Headers.Add("X-Proxied", "true");
                await Task.CompletedTask;
            });
    }
}
```

### SPA / SSG Hosting

Host single-page applications or static sites with client-side routing support. `LocalStaticApp()` serves files from the local `wwwroot` directory and applies a fallback file when the requested path does not exist.

Use a catch-all route such as `{*path}` so the static app handler can resolve the requested virtual path.

```csharp
public class StaticWebsite(IHttpContextAccessor httpContextAccessor) : HttpFunctionBase(httpContextAccessor)
{
    [FunctionName(nameof(StaticWebsite))]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{*path}")] HttpRequest req)
    {
        // Serve from local wwwroot with SPA fallback
        return LocalStaticApp(fallbackPath: "200.html", fallbackExcludePattern: "^/_nuxt/.*");
    }
}
```

### App Service Authentication

When App Service Authentication (EasyAuth) is enabled, access the authenticated user via the `User` property.

```csharp
public class SecureFunction(IHttpContextAccessor httpContextAccessor) : HttpFunctionBase(httpContextAccessor)
{
    [Function("SecureFunction")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Unauthorized();
        }

        return Ok($"Hello, {User.Identity.Name}");
    }
}
```

## Available Helpers

| Method | Description |
|--------|-------------|
| `Ok()` / `Ok(value)` | Returns 200 OK |
| `BadRequest()` / `BadRequest(ModelState)` | Returns 400 Bad Request |
| `Unauthorized()` | Returns 401 Unauthorized |
| `Forbid()` | Returns 403 Forbidden |
| `NotFound()` / `NotFound(value)` | Returns 404 Not Found |
| `Conflict()` / `Conflict(ModelState)` | Returns 409 Conflict |
| `NoContent()` | Returns 204 No Content |
| `StatusCode(code)` | Returns a custom status code |
| `Content(string, contentType)` | Returns content with a specific content type |
| `File(path)` / `File(bytes, contentType)` / `File(stream, contentType)` | Returns a file response |
| `CreatedAtFunction(functionName, routeValues, value)` | Returns 201 with a Location header pointing to the specified function |
| `AcceptedAtFunction(functionName, routeValues, value)` | Returns 202 with a Location header pointing to the specified function |
| `TryValidateModel(model)` | Validates the model using DataAnnotations |
| `ValidationProblem(ModelState)` | Returns RFC 7807 Problem Details for validation errors |
| `Problem(detail, instance, statusCode, title, type)` | Returns RFC 7807 Problem Details |
| `Proxy(backendUri, beforeSend, afterSend)` | Forwards the request to a backend service with optional async hooks |
| `LocalStaticApp(defaultFile, fallbackPath, fallbackExcludePattern)` | Serves static files from local `wwwroot` with SPA fallback |

## License

This project is licensed under the [MIT License](https://github.com/shibayan/azure-functions-http-api/blob/master/LICENSE)
