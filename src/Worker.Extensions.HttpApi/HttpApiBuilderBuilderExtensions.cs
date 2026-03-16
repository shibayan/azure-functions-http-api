using Azure.Functions.Worker.Extensions.HttpApi.Internal;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Azure.Functions.Worker.Extensions.HttpApi;

public static class HttpApiBuilderBuilderExtensions
{
    public static IFunctionsWorkerApplicationBuilder ConfigureHttpApiExtension(this IFunctionsWorkerApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddHttpContextAccessor();

        return builder.UseMiddleware<InitializerMiddleware>()
                      .UseMiddleware<HttpContextAccessorMiddleware>()
                      .UseMiddleware<AppServiceAuthenticationMiddleware>();
    }
}
