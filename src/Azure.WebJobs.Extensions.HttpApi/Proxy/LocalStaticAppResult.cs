using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace Azure.WebJobs.Extensions.HttpApi.Proxy
{
    internal class LocalStaticAppResult : IActionResult
    {
        public LocalStaticAppResult(string virtualPath)
        {
            VirtualPath = virtualPath;
        }

        public string VirtualPath { get; set; }

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
