using Azure.WebJobs.Extensions.HttpApi.Internal;

using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Hosting;

namespace Azure.WebJobs.Extensions.HttpApi.Routing;

public class RoutePrecedenceExtensionConfigProvider : IExtensionConfigProvider
{
    private readonly IWebJobsRouter _router;

    public RoutePrecedenceExtensionConfigProvider(IHostApplicationLifetime hostApplicationLifetime, IWebJobsRouter router)
    {
        _router = router;

        hostApplicationLifetime.ApplicationStarted.Register(PrecedenceRoutes);
    }

    public void Initialize(ExtensionConfigContext _) { }

    public void PrecedenceRoutes()
    {
        var routes = _router.GetRoutes();

        if (routes.Count is <= 1)
        {
            return;
        }

        var functionRoutes = new RouteCollection();

        foreach (var route in routes.OrderBy(x => RoutePrecedence.ComputeInbound(x.ParsedTemplate)))
        {
            functionRoutes.Add(route);
        }

        _router.ClearRoutes();
        _router.AddFunctionRoutes(functionRoutes, null);
    }
}
