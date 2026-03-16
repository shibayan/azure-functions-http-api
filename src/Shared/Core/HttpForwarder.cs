using System.Net;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Azure.WebJobs.Extensions.HttpApi.Internal;

internal sealed class HttpForwarder
{
    public async Task SendAsync(string destinationUri, HttpContext httpContext, Func<HttpRequestMessage, Task>? beforeSend = null, Func<HttpResponseMessage, Task>? afterSend = null)
    {
        ArgumentNullException.ThrowIfNull(destinationUri);
        ArgumentNullException.ThrowIfNull(httpContext);

        using var request = new HttpRequestMessage();

        request.Method = GetHttpMethod(httpContext.Request.Method);
        request.RequestUri = new Uri(destinationUri);

        if (HasRequestBody(httpContext))
        {
            request.Content = new StreamContent(httpContext.Request.Body);
        }

        CopyRequestHeaders(httpContext, request);

        if (beforeSend is not null)
        {
            await beforeSend(request);
        }

        using var response = await s_httpClient.SendAsync(request, httpContext.RequestAborted);

        if (afterSend is not null)
        {
            await afterSend(response);
        }

        httpContext.Response.StatusCode = (int)response.StatusCode;

        CopyResponseHeaders(httpContext, response);

        await response.Content.CopyToAsync(httpContext.Response.Body, httpContext.RequestAborted);
    }

    private static HttpMethod GetHttpMethod(string method)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(method);

        return new HttpMethod(method);
    }

    private static bool HasRequestBody(HttpContext httpContext)
    {
        var request = httpContext.Request;
        var bodyDetectionFeature = httpContext.Features.Get<IHttpRequestBodyDetectionFeature>();

        if (bodyDetectionFeature?.CanHaveBody == true)
        {
            return true;
        }

        if (request.Headers.TransferEncoding.Count > 0)
        {
            return true;
        }

        foreach (var (name, _) in request.Headers)
        {
            if (name.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void CopyRequestHeaders(HttpContext httpContext, HttpRequestMessage request)
    {
        var connectionHeaderValues = httpContext.Request.Headers.Connection;

        foreach (var (name, value) in httpContext.Request.Headers)
        {
            if (ShouldSkipHeader(name, connectionHeaderValues))
            {
                continue;
            }

            if (!request.Headers.TryAddWithoutValidation(name, (string?)value))
            {
                request.Content?.Headers.TryAddWithoutValidation(name, (string?)value);
            }
        }
    }

    private static void CopyResponseHeaders(HttpContext httpContext, HttpResponseMessage response)
    {
        var connectionHeaderValues = response.Headers.Connection;

        foreach (var (name, value) in response.Headers)
        {
            if (ShouldSkipHeader(name, connectionHeaderValues))
            {
                continue;
            }

            httpContext.Response.Headers[name] = value.ToArray();
        }

        foreach (var (name, value) in response.Content.Headers)
        {
            if (ShouldSkipHeader(name, connectionHeaderValues))
            {
                continue;
            }

            httpContext.Response.Headers[name] = value.ToArray();
        }
    }

    private static bool ShouldSkipHeader(string name, IEnumerable<string> connectionHeaderValues)
    {
        if (s_skipHeaders.Contains(name))
        {
            return true;
        }

        foreach (var value in connectionHeaderValues)
        {
            foreach (var token in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (string.Equals(token, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static readonly HttpMessageInvoker s_httpClient = new(new SocketsHttpHandler
    {
        AllowAutoRedirect = false,
        AutomaticDecompression = DecompressionMethods.None,
        UseCookies = false,
        UseProxy = false
    });

    private static readonly HashSet<string> s_skipHeaders = new([
        "Connection",
        "Host",
        "Keep-Alive",
        "Proxy-Authenticate",
        "Proxy-Authorization",
        "TE",
        "Trailer",
        "Transfer-Encoding",
        "Upgrade"
    ], StringComparer.OrdinalIgnoreCase);
}
