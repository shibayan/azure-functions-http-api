using System.Net;
using System.Text.RegularExpressions;

using Azure.WebJobs.Extensions.HttpApi.Internal;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Azure.WebJobs.Extensions.HttpApi.Core;

internal sealed partial class ProxyResultExecutor : IActionResultExecutor<ProxyResult>
{
    private static readonly HttpForwarder s_httpForwarder = new();

    public async Task ExecuteAsync(ActionContext context, ProxyResult result)
    {
        try
        {
            var backendUri = BuildBackendUri(result.BackendUri, context);

            await s_httpForwarder.SendAsync(backendUri, context.HttpContext, result.BeforeSend, result.AfterSend);
        }
        catch (HttpRequestException)
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadGateway;
        }
        catch (OperationCanceledException) when (context.HttpContext.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected — no response needed
        }
        catch (Exception) when (!context.HttpContext.Response.HasStarted)
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
    }

    private static string BuildBackendUri(string backendUri, ActionContext context)
    {
        var routeValues = context.RouteData.Values;

        var generatedBackendUri = TemplateRegex().Replace(backendUri, m => routeValues[m.Groups[1].Value]?.ToString() ?? string.Empty);

        if (generatedBackendUri == backendUri)
        {
            if (routeValues.Count is 1)
            {
                var (_, value) = routeValues.First();

                generatedBackendUri += generatedBackendUri.EndsWith('/') ? value : $"/{value}";
            }
        }

        return generatedBackendUri;
    }

    [GeneratedRegex(@"\{([^\{\}]+)\}")]
    private static partial Regex TemplateRegex();
}
