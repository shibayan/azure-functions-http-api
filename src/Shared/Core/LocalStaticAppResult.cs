using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace Azure.WebJobs.Extensions.HttpApi.Core;

public class LocalStaticAppResult : IActionResult
{
    public string DefaultFile { get; set; }

    public string FallbackPath { get; set; }

    public string FallbackExclude { get; set; }

    private static readonly LocalStaticAppResultExecutor s_executor = new();

    public Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return s_executor.ExecuteAsync(context, this);
    }
}
