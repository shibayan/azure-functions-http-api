using System.Reflection;

using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Azure.WebJobs.Extensions.HttpApi.Internal;

internal static class WebJobsRouterExtensions
{
    private static readonly FieldInfo s_functionRoutesField = typeof(WebJobsRouter).GetField("_functionRoutes", BindingFlags.NonPublic | BindingFlags.Instance)
                                                              ?? throw new MissingFieldException(typeof(WebJobsRouter).FullName, "_functionRoutes");

    public static IReadOnlyList<Route> GetRoutes(this IWebJobsRouter router)
    {
        var functionRoutes = s_functionRoutesField.GetValue(router) as RouteCollection
                             ?? throw new InvalidOperationException("The WebJobs router does not contain a route collection.");

        return GetRoutes(functionRoutes);
    }

    private static IReadOnlyList<Route> GetRoutes(RouteCollection collection)
    {
        List<Route> routes = [];

        for (var i = 0; i < collection.Count; i++)
        {
            if (collection[i] is RouteCollection nestedCollection)
            {
                routes.AddRange(GetRoutes(nestedCollection));
                continue;
            }

            if (collection[i] is Route route)
            {
                routes.Add(route);
            }
        }

        return routes;
    }
}
