using Microsoft.AspNetCore.Http;

namespace Azure.WebJobs.Extensions.HttpApi;

public class HttpFunctionBase(IHttpContextAccessor httpContextAccessor) : Core.HttpFunctionImpl(httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor)));
