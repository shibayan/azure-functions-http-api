using System;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace Azure.WebJobs.Extensions.HttpApi
{
    public abstract class ProxyFunctionBase
    {
        protected ProxyFunctionBase(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpContextAccessor = httpContextAccessor;
        }

        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        protected async Task SendAsync(string destinationPrefix)
        {
            var context = _httpContextAccessor.HttpContext;

            var request = new HttpRequestMessage();

            request.Method = GetHttpMethod(context.Request.Method);

            foreach (var (name, value) in context.Request.Headers)
            {
                request.Headers.TryAddWithoutValidation(name, (string)value);
            }

            var response = await _httpClient.SendAsync(request);
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
    }
}
