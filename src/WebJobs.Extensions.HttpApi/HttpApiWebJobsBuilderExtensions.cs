using Azure.WebJobs.Extensions.HttpApi.Internal;

using Microsoft.Azure.WebJobs;

namespace Azure.WebJobs.Extensions.HttpApi;

public static class HttpApiWebJobsBuilderExtensions
{
    public static IWebJobsBuilder ConfigureHttpApiExtension(this IWebJobsBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddExtension<RoutePrecedenceExtensionConfigProvider>();

        return builder;
    }
}
