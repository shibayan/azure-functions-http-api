using Microsoft.AspNetCore.Mvc;

namespace Azure.WebJobs.Extensions.HttpApi.Core;

public class ProxyResult(string backendUri) : IActionResult
{
    public string BackendUri { get; } = backendUri ?? throw new ArgumentNullException(nameof(backendUri));

    public Func<HttpRequestMessage, Task>? BeforeSend { get; set; }

    public Func<HttpResponseMessage, Task>? AfterSend { get; set; }

    private static readonly ProxyResultExecutor s_executor = new();

    public Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return s_executor.ExecuteAsync(context, this);
    }
}
