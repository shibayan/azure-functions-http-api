using Microsoft.AspNetCore.Http;

namespace Azure.Functions.Worker.Extensions.HttpApi;

public class HttpFunctionBase(IHttpContextAccessor httpContextAccessor) : WebJobs.Extensions.HttpApi.Core.HttpFunctionImpl(httpContextAccessor)
{
}
