using System.Linq;

using Azure.WebJobs.Extensions.HttpApi.Internal;

using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Hosting;

namespace Azure.WebJobs.Extensions.HttpApi.Routing
{
    public class RoutePrecedenceExtensionConfigProvider : IExtensionConfigProvider
    {
        public RoutePrecedenceExtensionConfigProvider(IHostApplicationLifetime hostApplicationLifetime, IWebJobsRouter router)
        {
            _router = router;

            hostApplicationLifetime.ApplicationStarted.Register(PrecedenceRoutes);
        }

        private readonly IWebJobsRouter _router;

        public void Initialize(ExtensionConfigContext context)
        {
        }

        public void PrecedenceRoutes()
        {
            var routes = _router.GetRoutes();

            var functionRoutes = new RouteCollection();

            foreach (var route in routes.OrderBy(x => RoutePrecedence.ComputeInbound(x.ParsedTemplate)))
            {
                functionRoutes.Add(route);
            }

            _router.ClearRoutes();
            _router.AddFunctionRoutes(functionRoutes, null);
        }
    }
}
