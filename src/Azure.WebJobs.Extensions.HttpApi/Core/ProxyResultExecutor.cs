using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Azure.WebJobs.Extensions.HttpApi.Internal;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Azure.WebJobs.Extensions.HttpApi.Core
{
    internal class ProxyResultExecutor : IActionResultExecutor<ProxyResult>
    {
        private static readonly HttpForwarder s_httpForwarder = new();
        private static readonly Regex s_templateRegex = new(@"\{([^\{\}]+)\}", RegexOptions.Compiled);

        public async Task ExecuteAsync(ActionContext context, ProxyResult result)
        {
            try
            {
                var backendUri = MakeBackendUri(result.BackendUri, context);

                await s_httpForwarder.SendAsync(backendUri, context.HttpContext, result.BeforeSend, result.AfterSend);
            }
            catch
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        private static string MakeBackendUri(string backendUri, ActionContext context)
        {
            var routeValues = context.RouteData.Values;

            var generatedBackendUri = s_templateRegex.Replace(backendUri, m => routeValues[m.Groups[1].Value]?.ToString() ?? "");

            if (generatedBackendUri == backendUri)
            {
                if (routeValues.Count == 1)
                {
                    var (_, value) = routeValues.First();

                    generatedBackendUri += generatedBackendUri.EndsWith("/") ? value : $"/{value}";
                }
            }

            return generatedBackendUri;
        }
    }
}
