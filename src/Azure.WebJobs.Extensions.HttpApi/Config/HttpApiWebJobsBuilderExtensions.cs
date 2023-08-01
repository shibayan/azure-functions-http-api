using System;

using Azure.WebJobs.Extensions.HttpApi.Routing;

using Microsoft.Azure.WebJobs;

namespace Azure.WebJobs.Extensions.HttpApi.Config;

public static class HttpApiWebJobsBuilderExtensions
{
    public static IWebJobsBuilder AddHttpApi(this IWebJobsBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.AddExtension<RoutePrecedenceExtensionConfigProvider>();

        return builder;
    }
}
