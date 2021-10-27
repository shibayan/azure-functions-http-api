using Azure.WebJobs.Extensions.HttpApi;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace ProxySample
{
    public class StaticWebsite : HttpFunctionBase
    {
        public StaticWebsite(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }

        [FunctionName(nameof(StaticWebsite))]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", "options", "head", "patch", Route = "{*path}")]
            HttpRequest req)
        {
            return Proxy("https://ststaticwebsiteproxy.z11.web.core.windows.net/{path}", after: response =>
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    response.StatusCode = System.Net.HttpStatusCode.OK;
                }
            });
        }
    }
}
