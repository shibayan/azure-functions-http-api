using System.Net;

using Azure.WebJobs.Extensions.HttpApi.Internal;

namespace Azure.WebJobs.Extensions.HttpApi.Core;

public class RemoteStaticAppResult : ProxyResult
{
    public RemoteStaticAppResult(string backendUri)
        : base(backendUri)
    {
        AfterSend = AfterSendInternal;
    }

    public string? FallbackExclude { get; set; }

    private void AfterSendInternal(HttpResponseMessage response)
    {
        var request = response.RequestMessage ?? throw new InvalidOperationException("The proxied response did not include the original request message.");
        var requestUri = request.RequestUri ?? throw new InvalidOperationException("The proxied request did not include a request URI.");

        // Try Fallback
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            if (string.IsNullOrEmpty(FallbackExclude) || !RegexCache.IsMatch(requestUri.AbsolutePath, FallbackExclude))
            {
                response.StatusCode = HttpStatusCode.OK;
            }
        }
    }
}
