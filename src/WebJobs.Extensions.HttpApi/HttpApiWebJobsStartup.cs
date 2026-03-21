using Azure.WebJobs.Extensions.HttpApi;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(HttpApiWebJobsStartup))]

namespace Azure.WebJobs.Extensions.HttpApi;

public sealed class HttpApiWebJobsStartup : IWebJobsStartup
{
    public void Configure(IWebJobsBuilder builder) => builder.ConfigureHttpApiExtension();
}
