﻿using Azure.Functions.Worker.Extensions.HttpApi;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace IsolatedSample;

public class Function1(IHttpContextAccessor httpContextAccessor, ILogger<Function1> logger)
    : HttpFunctionBase(httpContextAccessor)
{
    private readonly ILogger _logger = logger;

    [Function("Function1")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        return Ok($"Welcome to Azure Functions, {req.Query["name"]}!");
    }
}
