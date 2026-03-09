using Microsoft.AspNetCore.Mvc;

namespace Azure.WebJobs.Extensions.HttpApi.Core;

public class ProxyResult(string backendUri) : IActionResult
{
    public string BackendUri { get; } = backendUri ?? throw new ArgumentNullException(nameof(backendUri));

    public Action<HttpRequestMessage>? BeforeSend { get; set; }

    public Action<HttpResponseMessage>? AfterSend { get; set; }

    private static readonly ProxyResultExecutor s_executor = new();

    public Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return s_executor.ExecuteAsync(context, this);
    }
}
