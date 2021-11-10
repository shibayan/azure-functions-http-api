using Azure.WebJobs.Extensions.HttpApi;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace WebsiteSample
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
            HttpRequest req,
            string path)
        {
            //return ProxySpa("https://ststaticwebsiteproxy.z11.web.core.windows.net/{path}", fallbackExclude: $"^/_nuxt/.*");

            return ServeSpa(path, fallbackPath: "200.html", fallbackExclude: $"^/_nuxt/.*");
        }
    }
}
