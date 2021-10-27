using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace Azure.WebJobs.Extensions.HttpApi.Proxy
{
    internal class ProxyResult : IActionResult
    {
        public ProxyResult(string backendUri)
        {
            _backendUri = backendUri;
        }

        private readonly string _backendUri;

        private static readonly Regex _templateRegex = new Regex(@"\{([^\{\}]+)\}", RegexOptions.Compiled);

        public ProxyInvoker ProxyInvoker { get; set; }

        public Action<HttpRequestMessage> Before { get; set; }

        public Action<HttpResponseMessage> After { get; set; }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            try
            {
                var backendUri = MakeBackendUri(context);

                await ProxyInvoker.SendAsync(backendUri, context.HttpContext, Before, After);
            }
            catch
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        private string MakeBackendUri(ActionContext context)
        {
            var routeValues = context.RouteData.Values;

            var backend = _templateRegex.Replace(_backendUri, match =>
            {
                if (routeValues.TryGetValue(match.Groups[1].Value, out var value) && value != null)
                {
                    return value.ToString();
                }

                return "";
            });

            return backend;
        }
    }
}
