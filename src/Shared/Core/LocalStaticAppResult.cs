using Microsoft.AspNetCore.Mvc;

namespace Azure.WebJobs.Extensions.HttpApi.Core;

/// <summary>
/// An <see cref="IActionResult"/> that serves static files from the local <c>wwwroot</c> directory
/// with SPA-style fallback support. When the requested path is not found, it falls back to
/// <see cref="FallbackPath"/> unless the path matches <see cref="FallbackExcludePattern"/>.
/// </summary>
public class LocalStaticAppResult : IActionResult
{
    public required string RequestPath { get; set; }

    public string DefaultFile { get; set; } = "index.html";

    public string FallbackPath { get; set; } = "404.html";

    public string? FallbackExcludePattern { get; set; }

    private static readonly LocalStaticAppResultExecutor s_executor = new();

    public Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return s_executor.ExecuteAsync(context, this);
    }
}
