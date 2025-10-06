using Azure.Functions.Worker.Extensions.HttpApi;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace IsolatedSample;

public class Function2(IHttpContextAccessor httpContextAccessor, ILogger<Function2> logger)
    : HttpFunctionBase(httpContextAccessor)
{
    [Function("Function2")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Unauthorized();
        }

        logger.LogInformation("C# HTTP trigger function processed a request.");

        return Ok($"Hello, {User.Identity.Name}");
    }
}
