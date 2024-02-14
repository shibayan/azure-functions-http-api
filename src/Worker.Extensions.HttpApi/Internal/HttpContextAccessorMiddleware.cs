using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Azure.Functions.Worker.Extensions.HttpApi.Internal;

internal class HttpContextAccessorMiddleware : IFunctionsWorkerMiddleware
{
    public HttpContextAccessorMiddleware(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private readonly IHttpContextAccessor _httpContextAccessor;

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        _httpContextAccessor.HttpContext = context.GetHttpContext();

        await next(context);
    }
}
