using System;

using Azure.Functions.Worker.Extensions.HttpApi.Internal;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Azure.Functions.Worker.Extensions.HttpApi.Config;

public static class HttpApiBuilderExtensions
{
    public static IFunctionsWorkerApplicationBuilder AddHttpApi(this IFunctionsWorkerApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddHttpContextAccessor();

        builder.UseMiddleware<InitializerMiddleware>();
        builder.UseMiddleware<HttpContextAccessorMiddleware>();
        builder.UseMiddleware<AppServiceAuthenticationMiddleware>();

        return builder;
    }
}
