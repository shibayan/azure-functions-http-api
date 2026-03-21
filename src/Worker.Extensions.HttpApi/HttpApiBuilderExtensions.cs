using Azure.Functions.Worker.Extensions.HttpApi.Internal;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#pragma warning disable IDE0130
namespace Microsoft.Azure.Functions.Worker;
#pragma warning restore IDE0130

public static class HttpApiBuilderExtensions
{
    public static IFunctionsWorkerApplicationBuilder ConfigureHttpApiExtension(this IFunctionsWorkerApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddHttpContextAccessor();

        return builder.UseMiddleware<EndpointRouteNameMetadataMiddleware>()
                      .UseMiddleware<HttpContextAccessorMiddleware>()
                      .UseMiddleware<AppServiceAuthenticationMiddleware>();
    }
}
