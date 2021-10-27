using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Azure.WebJobs.Extensions.HttpApi.Proxy
{
    internal class StaticWebsiteResult : ProxyResult
    {
        public StaticWebsiteResult(string backendUri)
            : base(backendUri)
        {
            After = HandleAfter;
        }

        public string FallbackExclude { get; set; }

        private void HandleAfter(HttpResponseMessage response)
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
}
