using System.Reflection;

using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Azure.WebJobs.Extensions.HttpApi.Internal;

internal static class WebJobsRouterExtensions
{
    private static readonly FieldInfo s_functionRoutesField = ResolveFunctionRoutesField();

    public static IReadOnlyList<Route> GetRoutes(this IWebJobsRouter router)
    {
        ArgumentNullException.ThrowIfNull(router);

        var functionRoutes = s_functionRoutesField.GetValue(router) as RouteCollection
                             ?? throw new InvalidOperationException($"Failed to read routes from '{router.GetType().FullName}'. Expected the internal field '{s_functionRoutesField.Name}' to contain a '{nameof(RouteCollection)}'. The current Azure WebJobs runtime may be incompatible with this package.");

        return GetRoutes(functionRoutes);
    }

    private static FieldInfo ResolveFunctionRoutesField()
        => typeof(WebJobsRouter).GetField("_functionRoutes", BindingFlags.NonPublic | BindingFlags.Instance)
           ?? throw new InvalidOperationException($"Failed to access the internal field '{typeof(WebJobsRouter).FullName}._functionRoutes'. The current Azure WebJobs runtime may be incompatible with this package.");

    private static IReadOnlyList<Route> GetRoutes(RouteCollection collection)
    {
        List<Route> routes = [];

        for (var i = 0; i < collection.Count; i++)
        {
            switch (collection[i])
            {
                case RouteCollection nestedCollection:
                    routes.AddRange(GetRoutes(nestedCollection));
                    continue;
                case Route route:
                    routes.Add(route);
                    break;
            }
        }

        return routes;
    }
}
