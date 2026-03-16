using System.Reflection;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Hosting;

namespace Azure.Functions.Worker.Extensions.HttpApi.Internal;

internal sealed class InitializerMiddleware(IServiceProvider serviceProvider) : IFunctionsWorkerMiddleware
{
    private volatile bool _initialized;

    private static readonly Lazy<(Type DataSourceType, FieldInfo EndpointsField)> s_reflectionInfo = new(ResolveReflectionInfo);

    public Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (!_initialized && context.GetHttpContext() is not null)
        {
            TryInitialize();
        }

        return next(context);
    }

    private void TryInitialize()
    {
        var (dataSourceType, endpointsField) = s_reflectionInfo.Value;

        var dataSource = serviceProvider.GetService(dataSourceType);

        if (dataSource is null)
        {
            return;
        }

        if (endpointsField.GetValue(dataSource) is not List<Endpoint> endpoints)
        {
            return;
        }

        List<Endpoint> newEndpoints = [];

        foreach (var endpoint in endpoints)
        {
            if (endpoint is not RouteEndpoint routeEndpoint)
            {
                throw new InvalidOperationException($"Failed to initialize HTTP API routes because endpoint '{endpoint.DisplayName ?? endpoint.GetType().FullName}' is not a '{nameof(RouteEndpoint)}'.");
            }

            var builder = new RouteEndpointBuilder(routeEndpoint.RequestDelegate, routeEndpoint.RoutePattern, routeEndpoint.Order)
            {
                DisplayName = routeEndpoint.DisplayName
            };

            foreach (var metadata in routeEndpoint.Metadata)
            {
                builder.Metadata.Add(metadata);
            }

            builder.Metadata.Add(new RouteNameMetadata(routeEndpoint.DisplayName ?? routeEndpoint.RoutePattern.RawText ?? string.Empty));

            newEndpoints.Add(builder.Build());
        }

        endpointsField.SetValue(dataSource, newEndpoints);

        _initialized = true;
    }

    private static (Type DataSourceType, FieldInfo EndpointsField) ResolveReflectionInfo()
    {
        var dataSourceType = typeof(FunctionsHostBuilderExtensions).Assembly.GetTypes().FirstOrDefault(x => x.Name == "FunctionsEndpointDataSource")
            ?? throw new InvalidOperationException($"Failed to locate the internal Azure Functions endpoint data source type in assembly '{typeof(FunctionsHostBuilderExtensions).Assembly.FullName}'. The current Azure Functions worker runtime may be incompatible with this package.");

        var endpointsField = dataSourceType.GetField("_endpoints", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Failed to access the internal field '{dataSourceType.FullName}._endpoints'. The current Azure Functions worker runtime may be incompatible with this package.");

        return (dataSourceType, endpointsField);
    }
}
