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

            return _templateRegex.Replace(backendUri, match =>
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
