using Azure.WebJobs.Extensions.HttpApi.Config;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(Azure.WebJobs.Extensions.HttpApi.HttpApiWebJobsStartup))]

namespace Azure.WebJobs.Extensions.HttpApi;

public sealed class HttpApiWebJobsStartup : IWebJobsStartup2
{
    public void Configure(IWebJobsBuilder builder) => builder.AddHttpApi();

    public void Configure(WebJobsBuilderContext _, IWebJobsBuilder builder) => Configure(builder);
}
