using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace Azure.WebJobs.Extensions.HttpApi.Core
{
    internal class LocalStaticAppResult : IActionResult
    {
        public string DefaultFile { get; set; }

        public string FallbackPath { get; set; }

        public string FallbackExclude { get; set; }

        private static readonly LocalStaticAppResultExecutor _executor = new();

        public Task ExecuteResultAsync(ActionContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return _executor.ExecuteAsync(context, this);
        }
    }
}
