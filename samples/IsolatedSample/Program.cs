using Azure.Functions.Worker.Extensions.HttpApi.Config;

using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(workerApplication =>
    {
        workerApplication.AddHttpApi();
    })
    .Build();

host.Run();
