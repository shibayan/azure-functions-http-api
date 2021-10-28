using System;
using System.Net.Http;

namespace Azure.WebJobs.Extensions.HttpApi.Proxy
{
    internal class ProxyResult : ProxyResultBase
    {
        public ProxyResult(string backendUri)
            : base(backendUri)
        {
        }

        public Action<HttpRequestMessage> Before { get; set; }

        public Action<HttpResponseMessage> After { get; set; }

        protected override void BeforeSend(HttpRequestMessage request) => Before?.Invoke(request);

        protected override void AfterSend(HttpResponseMessage response) => After?.Invoke(response);
    }
}
