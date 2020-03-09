using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HttpApi
{
    public class Function4 : HttpFunctionBase
    {
        public Function4(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        [FunctionName("Function4")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            SampleModel model,
            ILogger log)
        {
            if (!TryValidateModel(model))
            {
                return ValidationProblem(ModelState);
            }

            return Ok(model);
        }
    }
}
