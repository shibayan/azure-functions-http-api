using Azure.WebJobs.Extensions.HttpApi;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace BasicSample
{
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
}
