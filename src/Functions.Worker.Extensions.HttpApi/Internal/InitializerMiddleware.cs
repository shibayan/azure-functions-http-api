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

internal class InitializerMiddleware : IFunctionsWorkerMiddleware
{
    public InitializerMiddleware(IServiceProvider serviceProvider) => Initialize(serviceProvider);

    public Task Invoke(FunctionContext context, FunctionExecutionDelegate next) => next(context);

    private void Initialize(IServiceProvider serviceProvider)
    {
        var type = typeof(FunctionsHostBuilderExtensions).Assembly.GetTypes().First(x => x.Name == "FunctionsEndpointDataSource");

        var dataSource = serviceProvider.GetService(type);

        var field = type.GetField("_endpoints", BindingFlags.NonPublic | BindingFlags.Instance);

        var endpoints = (List<Endpoint>)field.GetValue(dataSource);

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
    }
}
