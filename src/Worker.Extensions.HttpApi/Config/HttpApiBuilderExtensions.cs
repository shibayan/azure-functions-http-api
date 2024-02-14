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
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.AddHttpContextAccessor();
        builder.UseMiddleware<HttpContextAccessorMiddleware>();

        return builder;
    }
}
