using System;

using Azure.WebJobs.Extensions.HttpApi;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace BasicSample
{
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
}
