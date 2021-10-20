using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace Azure.WebJobs.Extensions.HttpApi
{
    internal class ProxyResult : IActionResult
    {
        public ProxyResult(string template)
        {
            _template = template;
        }

        private readonly string _template;

        private static readonly Regex _templateRegex = new Regex(@"\{([^\{\}]+)\}", RegexOptions.Compiled);

        public ProxyInvoker ProxyInvoker { get; set; }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            try
            {
                var backend = MakeBackendUrl(context);

                await ProxyInvoker.SendAsync(backend, context.HttpContext);
            }
            catch
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        private string MakeBackendUrl(ActionContext context)
        {
            var routeValues = context.RouteData.Values;

            var backend = _templateRegex.Replace(_template, match =>
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
