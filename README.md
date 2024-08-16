# HTTP API Extensions for Azure Functions

[![Build](https://github.com/shibayan/azure-functions-http-api/workflows/Build/badge.svg)](https://github.com/shibayan/azure-functions-http-api/actions/workflows/build.yml)
[![Downloads](https://badgen.net/nuget/dt/WebJobs.Extensions.HttpApi)](https://www.nuget.org/packages/WebJobs.Extensions.HttpApi/)
[![NuGet](https://badgen.net/nuget/v/WebJobs.Extensions.HttpApi)](https://www.nuget.org/packages/WebJobs.Extensions.HttpApi/)
[![License](https://badgen.net/github/license/shibayan/azure-functions-http-api)](https://github.com/shibayan/azure-functions-http-api/blob/master/LICENSE)

## Features

- Better route precedence
- Model validation
- ASP.NET Core like helpers
- Support URL generation
- Handle static files
- Simple reverse proxy
- Streamlined SPA / SSG hosting

## Installation

```
# For .NET 6/8 In-Process
Install-Package WebJobs.Extensions.HttpApi

# For .NET Isolated Worker
Install-Package Functions.Worker.Extensions.HttpApi
```

```
# For .NET 6/8 In-Process
dotnet add package WebJobs.Extensions.HttpApi

# For .NET Isolated Worker
dotnet add package Functions.Worker.Extensions.HttpApi
```

```csharp
// Inherits from `HttpFunctionBase` class
public class Function1(IHttpContextAccessor httpContextAccessor) : HttpFunctionBase(httpContextAccessor)
{
    [FunctionName("Function1")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req,
        ILogger log)
    {
        return Ok($"Hello, {req.Query["name"]}");
    }
}
```

## Examples

### Model validation

```csharp
public class Function1(IHttpContextAccessor httpContextAccessor) : HttpFunctionBase(httpContextAccessor)
{
    [FunctionName("Function1")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")]
        SampleModel model,
        ILogger log)
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

### ASP.NET Core like helpers

```csharp
public class Function2(IHttpContextAccessor httpContextAccessor) : HttpFunctionBase(httpContextAccessor)
{
    [FunctionName("Function2")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get")]
        HttpRequest req,
        ILogger log)
    {
        Response.Headers.Add("Cache-Control", "no-cache");

        return Ok($"Now: {DateTime.Now}");
    }
}
```

### Support URL generation

```csharp
public class Function3(IHttpContextAccessor httpContextAccessor) : HttpFunctionBase(httpContextAccessor)
{
    [FunctionName("Function3")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "route/{id}")]
        HttpRequest req,
        string id,
        ILogger log)
    {
        return CreatedAtFunction("Function3", new { id = "kazuakix" }, null);
    }
}
```

### Handle static files

```csharp
public class Function1(IHttpContextAccessor httpContextAccessor) : HttpFunctionBase(httpContextAccessor)
{
    [FunctionName("Function1")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
        ILogger log)
    {
        return File("sample.html");
    }
}
```

### Simple reverse proxy

```csharp
public class Function1(IHttpContextAccessor httpContextAccessor) : HttpFunctionBase(httpContextAccessor)
{
    [FunctionName("Function1")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "{*path}"})] HttpRequest req,
        ILogger log)
    {
        return Proxy("https://example.com/{path}");
    }
}
```

### Streamlined SPA / SSG hosting

```csharp
public class Function1(IHttpContextAccessor httpContextAccessor) : HttpFunctionBase(httpContextAccessor)
{
    [FunctionName("Function1")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "{*path}"})] HttpRequest req,
        ILogger log)
    {
#if USE_REMOTE
        return RemoteStaticApp("https://example.com", fallbackExclude: $"^/_nuxt/.*");
#else
        return LocalStaticApp(fallbackPath: "404.html", fallbackExclude: $"^/_nuxt/.*");
#endif
    }
}
```

## License

This project is licensed under the [MIT License](https://github.com/shibayan/azure-functions-http-api/blob/master/LICENSE)
