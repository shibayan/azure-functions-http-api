using Azure.Functions.Worker.Extensions.HttpApi;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Core;

[assembly: WorkerExtensionStartup(typeof(HttpApiExtensionStartup))]

namespace Azure.Functions.Worker.Extensions.HttpApi;

public sealed class HttpApiExtensionStartup : WorkerExtensionStartup
{
    public override void Configure(IFunctionsWorkerApplicationBuilder applicationBuilder) => applicationBuilder.ConfigureHttpApiExtension();
}
