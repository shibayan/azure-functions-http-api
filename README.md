# HTTP API Extensions for Azure Functions

[![Build](https://github.com/shibayan/azure-functions-http-api/workflows/Build/badge.svg)](https://github.com/shibayan/azure-functions-http-api/actions/workflows/build.yml)
[![Downloads](https://img.shields.io/nuget/dt/WebJobs.Extensions.HttpApi)](https://www.nuget.org/packages/WebJobs.Extensions.HttpApi/)
[![NuGet](https://img.shields.io/nuget/v/WebJobs.Extensions.HttpApi)](https://www.nuget.org/packages/WebJobs.Extensions.HttpApi/)
[![License](https://img.shields.io/github/license/shibayan/azure-functions-http-api)](https://github.com/shibayan/azure-functions-http-api/blob/master/LICENSE)

## Features

- Model validation
- ASP.NET Core like helpers
- Support URL generation
- Handle static files
- Reverse proxy

## Basic usage

### Model validation

```csharp
public class Function1 : HttpFunctionBase
{
    public Function1(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }

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
public class Function2 : HttpFunctionBase
{
    public Function2(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }

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
public class Function3 : HttpFunctionBase
{
    public Function3(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }

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
public class Function1 : HttpFunctionBase
{
    public Function1(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }

    [FunctionName("Function1")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req,
        ILogger log)
    {
        return File("sample.html");
    }
}
```

### Reverse proxy

```csharp
public class Function1 : HttpFunctionBase
{
    public Function1(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }

    [FunctionName("Function1")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "{*path}"})] HttpRequest req,
        ILogger log)
    {
        return Proxy("https://example.com/{path}");
    }
}
```

## License

This project is licensed under the [MIT License](https://github.com/shibayan/azure-functions-http-api/blob/master/LICENSE)
