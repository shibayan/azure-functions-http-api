using System.Reflection;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Hosting;

namespace Azure.Functions.Worker.Extensions.HttpApi.Internal;

internal sealed class InitializerMiddleware(IServiceProvider serviceProvider) : IFunctionsWorkerMiddleware
{
    private bool _initialized;
    private readonly object _initializeLock = new();

    private static readonly Type s_endpointDataSourceType = typeof(FunctionsHostBuilderExtensions).Assembly.GetTypes().FirstOrDefault(x => x.Name == "FunctionsEndpointDataSource")
                                                        ?? throw new InvalidOperationException("The Azure Functions endpoint data source type could not be found.");
    private static readonly FieldInfo s_endpointsField = s_endpointDataSourceType.GetField("_endpoints", BindingFlags.NonPublic | BindingFlags.Instance)
                                                        ?? throw new MissingFieldException(s_endpointDataSourceType.FullName, "_endpoints");

    public Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpContext = context.GetHttpContext();

        if (!_initialized && httpContext is not null)
        {
            lock (_initializeLock)
            {
                if (!_initialized)
                {
                    _initialized = TryInitialize();
                }
            }
        }

        return next(context);
    }

    private bool TryInitialize()
    {
        var dataSource = serviceProvider.GetService(s_endpointDataSourceType);

        if (dataSource is null)
        {
            return false;
        }

        var endpoints = s_endpointsField.GetValue(dataSource) as List<Endpoint>;

        if (endpoints is null)
        {
            return false;
        }

        List<Endpoint> newEndpoints = [];

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

            builder.Metadata.Add(new RouteNameMetadata(endpoint.DisplayName ?? endpoint.RoutePattern.RawText ?? string.Empty));

            newEndpoints.Add(builder.Build());
        }

        s_endpointsField.SetValue(dataSource, newEndpoints);

        return true;
    }
}
