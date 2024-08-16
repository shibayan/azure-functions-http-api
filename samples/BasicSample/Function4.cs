using Azure.WebJobs.Extensions.HttpApi;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace BasicSample;

public class Function4(IHttpContextAccessor httpContextAccessor) : HttpFunctionBase(httpContextAccessor)
{
    [FunctionName(nameof(Function4))]
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
