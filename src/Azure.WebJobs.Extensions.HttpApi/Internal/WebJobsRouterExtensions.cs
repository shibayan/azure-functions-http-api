using System.Collections.Generic;
using System.Reflection;

using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Azure.WebJobs.Extensions.HttpApi.Internal;

internal static class WebJobsRouterExtensions
{
    public static IReadOnlyList<Route> GetRoutes(this IWebJobsRouter router)
    {
        var field = typeof(WebJobsRouter).GetField("_functionRoutes", BindingFlags.NonPublic | BindingFlags.Instance);
        var functionRoutes = (RouteCollection)field.GetValue(router);

        return GetRoutes(functionRoutes);
    }

    private static IReadOnlyList<Route> GetRoutes(RouteCollection collection)
    {
        var routes = new List<Route>();

        for (var i = 0; i < collection.Count; i++)
        {
            if (collection[i] is RouteCollection nestedCollection)
            {
                routes.AddRange(GetRoutes(nestedCollection));
                continue;
            }

            routes.Add((Route)collection[i]);
        }

        return routes;
    }
}
