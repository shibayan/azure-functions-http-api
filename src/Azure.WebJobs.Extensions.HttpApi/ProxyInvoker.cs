using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace Azure.WebJobs.Extensions.HttpApi
{
    internal class ProxyInvoker
    {
        public async Task SendAsync(string destinationUri, HttpContext httpContext)
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

            foreach (var (name, value) in httpContext.Request.Headers)
            {
                if (_skipHeaders.Contains(name))
                {
                    continue;
                }

                request.Headers.TryAddWithoutValidation(name, (string)value);
            }

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, httpContext.RequestAborted);

            httpContext.Response.StatusCode = (int)response.StatusCode;

            foreach (var (name, value) in response.Headers)
            {
                if (_skipHeaders.Contains(name))
                {
                    continue;
                }

                httpContext.Response.Headers.Add(name, value.ToArray());
            }

            foreach (var (name, value) in response.Content.Headers)
            {
                if (_skipHeaders.Contains(name))
                {
                    continue;
                }

                httpContext.Response.Headers.Add(name, value.ToArray());
            }

            await response.Content.CopyToAsync(httpContext.Response.Body);
        }

        private static bool HasRequestBody(HttpContext httpContext)
        {
            var method = httpContext.Request.Method;
            var contentLength = httpContext.Request.Headers.ContentLength ?? 0;

            if (contentLength > 0)
            {
                return true;
            }

            if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsDelete(method))
            {
                return false;
            }

            return true;
        }

        private static HttpMethod GetHttpMethod(string method)
        {
            return method switch
            {
                { } when HttpMethods.IsGet(method) => HttpMethod.Get,
                { } when HttpMethods.IsPost(method) => HttpMethod.Post,
                { } when HttpMethods.IsPut(method) => HttpMethod.Put,
                { } when HttpMethods.IsDelete(method) => HttpMethod.Delete,
                { } when HttpMethods.IsOptions(method) => HttpMethod.Options,
                { } when HttpMethods.IsHead(method) => HttpMethod.Head,
                { } when HttpMethods.IsPatch(method) => HttpMethod.Patch,
                _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
            };
        }

        private readonly HttpClient _httpClient = new HttpClient();

        private static readonly HashSet<string> _skipHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Host",
            "Connection",
            "Transfer-Encoding"
        };
    }
}
