using Azure.WebJobs.Extensions.HttpApi;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace ProxySample;

public class HttpTrigger1(IHttpContextAccessor httpContextAccessor) : HttpFunctionBase(httpContextAccessor)
{
    [FunctionName(nameof(HttpTrigger1))]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = $"api/{nameof(HttpTrigger1)}")] HttpRequest req,
        ILogger log)
    {
        return Ok(req.Query["name"]);
    }
}
