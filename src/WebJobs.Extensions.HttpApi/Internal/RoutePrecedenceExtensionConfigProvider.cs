using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Hosting;

namespace Azure.WebJobs.Extensions.HttpApi.Internal;

internal sealed class RoutePrecedenceExtensionConfigProvider : IExtensionConfigProvider
{
    private readonly IWebJobsRouter _router;

    public RoutePrecedenceExtensionConfigProvider(IHostApplicationLifetime hostApplicationLifetime, IWebJobsRouter router)
    {
        _router = router;

        hostApplicationLifetime.ApplicationStarted.Register(ApplyRoutePrecedence);
    }

    public void Initialize(ExtensionConfigContext context) { }

    public void ApplyRoutePrecedence()
    {
        var routes = _router.GetRoutes();

        if (routes.Count <= 1)
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
