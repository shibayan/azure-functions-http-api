using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Azure.Functions.Worker.Extensions.HttpApi.Internal;

internal sealed class HttpContextAccessorMiddleware(IHttpContextAccessor httpContextAccessor) : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpContext = context.GetHttpContext();

        if (httpContext is null)
        {
            await next(context);
            return;
        }

        httpContextAccessor.HttpContext = httpContext;

        try
        {
            await next(context);
        }
        finally
        {
            httpContextAccessor.HttpContext = null;
        }
    }
}
