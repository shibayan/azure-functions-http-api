using System;

using Azure.WebJobs.Extensions.HttpApi.Routing;

using Microsoft.Azure.WebJobs;

namespace Azure.WebJobs.Extensions.HttpApi.Config;

public static class HttpApiWebJobsBuilderExtensions
{
    public static IWebJobsBuilder AddHttpApi(this IWebJobsBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddExtension<RoutePrecedenceExtensionConfigProvider>();

        return builder;
    }
}
