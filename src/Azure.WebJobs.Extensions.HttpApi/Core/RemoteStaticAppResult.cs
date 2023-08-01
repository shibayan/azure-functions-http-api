using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Azure.WebJobs.Extensions.HttpApi.Core;

public class RemoteStaticAppResult : ProxyResult
{
    public RemoteStaticAppResult(string backendUri)
        : base(backendUri)
    {
        AfterSend = AfterSendInternal;
    }

    public string FallbackExclude { get; set; }

    private void AfterSendInternal(HttpResponseMessage response)
    {
        var request = response.RequestMessage;

        // Try Fallback
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            if (string.IsNullOrEmpty(FallbackExclude) || !Regex.IsMatch(request.RequestUri.AbsolutePath, FallbackExclude))
            {
                response.StatusCode = HttpStatusCode.OK;
            }
        }
    }
}
