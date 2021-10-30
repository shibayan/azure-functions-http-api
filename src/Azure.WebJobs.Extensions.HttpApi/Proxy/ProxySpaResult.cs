using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Azure.WebJobs.Extensions.HttpApi.Proxy
{
    internal class ProxySpaResult : ProxyResultBase
    {
        public ProxySpaResult(string backendUri)
            : base(backendUri)
        {
        }

        public string FallbackExclude { get; set; }

        protected override void AfterSend(HttpResponseMessage response)
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
