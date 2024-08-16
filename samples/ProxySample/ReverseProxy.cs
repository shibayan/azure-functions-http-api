using Azure.WebJobs.Extensions.HttpApi;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace ProxySample;

public class ReverseProxy(IHttpContextAccessor httpContextAccessor) : HttpFunctionBase(httpContextAccessor)
{
    [FunctionName(nameof(ReverseProxy))]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", "options", "head", "patch", Route = "{*path}")]
        HttpRequest req)
    {
        return Proxy("https://shibayan.jp");
    }
}
