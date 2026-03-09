using Microsoft.AspNetCore.Mvc;

namespace Azure.WebJobs.Extensions.HttpApi.Core;

public class LocalStaticAppResult : IActionResult
{
    public string DefaultFile { get; set; } = "index.html";

    public string FallbackPath { get; set; } = "404.html";

    public string? FallbackExclude { get; set; }

    private static readonly LocalStaticAppResultExecutor s_executor = new();

    public Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return s_executor.ExecuteAsync(context, this);
    }
}
