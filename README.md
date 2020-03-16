# HTTP API Extensions for Azure Functions v3

[![Build Status](https://dev.azure.com/shibayan/azure-functions-http-api/_apis/build/status/Build%20azure-functions-http-api?branchName=master)](https://dev.azure.com/shibayan/azure-functions-http-api/_build/latest?definitionId=57&branchName=master)
[![License](https://img.shields.io/github/license/shibayan/azure-functions-http-api.svg)](https://github.com/shibayan/azure-functions-http-api/blob/master/LICENSE)

## NuGet Packages

Package Name | Target Framework | NuGet
---|---|---
WebJobs.Extensions.HttpApi | .NET Core 3.1 | [![NuGet](https://img.shields.io/nuget/v/WebJobs.Extensions.HttpApi.svg)](https://www.nuget.org/packages/WebJobs.Extensions.HttpApi)

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

### ASP.NET Core like properties

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

## License

This project is licensed under the [MIT License](https://github.com/shibayan/azure-functions-http-api/blob/master/LICENSE)
