using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Hosting;

namespace Azure.Functions.Worker.Extensions.HttpApi.Internal;

internal class InitializerMiddleware(IServiceProvider serviceProvider) : IFunctionsWorkerMiddleware
{
    private bool _initialized;

    public Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpContext = context.GetHttpContext();

        if (!_initialized && httpContext is not null)
        {
            _initialized = TryInitialize();
        }

        return next(context);
    }

    private bool TryInitialize()
    {
        var type = typeof(FunctionsHostBuilderExtensions).Assembly.GetTypes().First(x => x.Name == "FunctionsEndpointDataSource");

        var dataSource = serviceProvider.GetService(type);

        var field = type.GetField("_endpoints", BindingFlags.NonPublic | BindingFlags.Instance);

        var endpoints = (List<Endpoint>)field.GetValue(dataSource);

        if (endpoints is null)
        {
            return false;
        }

        var newEndpoints = new List<Endpoint>();

        foreach (var endpoint in endpoints.Cast<RouteEndpoint>())
        {
            var builder = new RouteEndpointBuilder(endpoint.RequestDelegate, endpoint.RoutePattern, endpoint.Order)
            {
                DisplayName = endpoint.DisplayName
            };

            foreach (var metadata in endpoint.Metadata)
            {
                builder.Metadata.Add(metadata);
            }

            builder.Metadata.Add(new RouteNameMetadata(endpoint.DisplayName));

            newEndpoints.Add(builder.Build());
        }

        field.SetValue(dataSource, newEndpoints);

        return true;
    }
}
