using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Azure.WebJobs.Extensions.HttpApi.Proxy
{
    internal class ProxyResultExecutor : IActionResultExecutor<ProxyResult>
    {
        private static readonly HttpForwarder _httpForwarder = new();
        private static readonly Regex _templateRegex = new(@"\{([^\{\}]+)\}", RegexOptions.Compiled);

        public async Task ExecuteAsync(ActionContext context, ProxyResult result)
        {
            try
            {
                var backendUri = MakeBackendUri(result.BackendUri, context);

                await _httpForwarder.SendAsync(backendUri, context.HttpContext, result.BeforeSend, result.AfterSend);
            }
            catch
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        private static string MakeBackendUri(string backendUri, ActionContext context)
        {
            var routeValues = context.RouteData.Values;

            var generatedBackendUri = _templateRegex.Replace(backendUri, m => routeValues[m.Groups[1].Value]?.ToString() ?? "");

            if (generatedBackendUri == backendUri)
            {
                var (_, value) = routeValues.Single();

                generatedBackendUri += generatedBackendUri.EndsWith("/") ? value : $"/{value}";
            }

            return generatedBackendUri;
        }
    }
}
