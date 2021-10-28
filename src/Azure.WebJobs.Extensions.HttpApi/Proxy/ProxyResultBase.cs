using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace Azure.WebJobs.Extensions.HttpApi.Proxy
{
    internal class ProxyResultBase : IActionResult
    {
        protected ProxyResultBase(string backendUri)
        {
            _backendUri = backendUri;
        }

        private readonly string _backendUri;

        private static readonly Regex _templateRegex = new(@"\{([^\{\}]+)\}", RegexOptions.Compiled);

        public ProxyInvoker ProxyInvoker { get; set; }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            try
            {
                var backendUri = MakeBackendUri(context);

                await ProxyInvoker.SendAsync(backendUri, context.HttpContext, BeforeSend, AfterSend);
            }
            catch
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        protected virtual void BeforeSend(HttpRequestMessage request)
        {
        }

        protected virtual void AfterSend(HttpResponseMessage response)
        {
        }

        private string MakeBackendUri(ActionContext context)
        {
            var routeValues = context.RouteData.Values;

            return _templateRegex.Replace(_backendUri, match =>
            {
                if (routeValues.TryGetValue(match.Groups[1].Value, out var value) && value != null)
                {
                    return value.ToString();
                }

                return "";
            });
        }
    }
}
