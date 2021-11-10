using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace Azure.WebJobs.Extensions.HttpApi.Proxy
{
    internal class StaticFileResult : IActionResult
    {
        public string ContentRoot { get; set; }

        public string DefaultFile { get; set; }

        public string FallbackPath { get; set; }

        public string FallbackExclude { get; set; }

        private static readonly StaticFileResultExecutor _executor = new();

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
