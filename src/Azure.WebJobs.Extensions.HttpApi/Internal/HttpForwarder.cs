using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace Azure.WebJobs.Extensions.HttpApi.Internal;

internal class HttpForwarder
{
    public async Task SendAsync(string destinationUri, HttpContext httpContext, Action<HttpRequestMessage> beforeSend = null, Action<HttpResponseMessage> afterSend = null)
    {
        var request = new HttpRequestMessage
        {
            Method = GetHttpMethod(httpContext.Request.Method),
            RequestUri = new Uri(destinationUri)
        };

        if (HasRequestBody(httpContext))
        {
            request.Content = new StreamContent(httpContext.Request.Body);
        }

        CopyRequestHeaders(httpContext, request);

        beforeSend?.Invoke(request);

        var response = await _httpClient.SendAsync(request, httpContext.RequestAborted);

        afterSend?.Invoke(response);

        httpContext.Response.StatusCode = (int)response.StatusCode;

        CopyResponseHeaders(httpContext, response);

        await response.Content.CopyToAsync(httpContext.Response.Body);
    }

    private static HttpMethod GetHttpMethod(string method)
    {
        return method switch
        {
            not null when HttpMethods.IsGet(method) => HttpMethod.Get,
            not null when HttpMethods.IsPost(method) => HttpMethod.Post,
            not null when HttpMethods.IsPut(method) => HttpMethod.Put,
            not null when HttpMethods.IsDelete(method) => HttpMethod.Delete,
            not null when HttpMethods.IsOptions(method) => HttpMethod.Options,
            not null when HttpMethods.IsHead(method) => HttpMethod.Head,
            not null when HttpMethods.IsPatch(method) => HttpMethod.Patch,
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
        };
    }

    private static bool HasRequestBody(HttpContext httpContext)
    {
        var method = httpContext.Request.Method;
        var contentLength = httpContext.Request.Headers.ContentLength ?? 0;

        if (contentLength > 0)
        {
            return true;
        }

        return !HttpMethods.IsGet(method) && !HttpMethods.IsHead(method) && !HttpMethods.IsDelete(method);
    }

    private static void CopyRequestHeaders(HttpContext httpContext, HttpRequestMessage request)
    {
        foreach (var (name, value) in httpContext.Request.Headers)
        {
            if (s_skipHeaders.Contains(name))
            {
                continue;
            }

            if (!request.Headers.TryAddWithoutValidation(name, (string)value))
            {
                request.Content?.Headers.TryAddWithoutValidation(name, (string)value);
            }
        }
    }

    private static void CopyResponseHeaders(HttpContext httpContext, HttpResponseMessage response)
    {
        foreach (var (name, value) in response.Headers)
        {
            if (s_skipHeaders.Contains(name))
            {
                continue;
            }

            httpContext.Response.Headers.TryAdd(name, value.ToArray());
        }

        foreach (var (name, value) in response.Content.Headers)
        {
            if (s_skipHeaders.Contains(name))
            {
                continue;
            }

            httpContext.Response.Headers.TryAdd(name, value.ToArray());
        }
    }

    private readonly HttpMessageInvoker _httpClient = new(new SocketsHttpHandler
    {
        AllowAutoRedirect = false,
        AutomaticDecompression = DecompressionMethods.None,
        UseCookies = false,
        UseProxy = false
    });

    private static readonly HashSet<string> s_skipHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Host",
        "Connection",
        "Transfer-Encoding"
    };
}
