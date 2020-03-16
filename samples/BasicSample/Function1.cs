using System.ComponentModel.DataAnnotations;

using Azure.WebJobs.Extensions.HttpApi;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace BasicSample
{
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
}
