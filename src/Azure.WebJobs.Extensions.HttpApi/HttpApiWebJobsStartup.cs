using Azure.WebJobs.Extensions.HttpApi.Config;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(Azure.WebJobs.Extensions.HttpApi.HttpApiWebJobsStartup))]

namespace Azure.WebJobs.Extensions.HttpApi
{
    public class HttpApiWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder) => builder.AddHttpApi();
    }
}
