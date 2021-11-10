using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Azure.WebJobs.Extensions.HttpApi.Proxy
{
    internal class StaticFileResultExecutor : IActionResultExecutor<StaticFileResult>
    {
        public Task ExecuteAsync(ActionContext context, StaticFileResult result)
        {
            return Task.CompletedTask;
        }
    }
}
