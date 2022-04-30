﻿using System;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace Azure.WebJobs.Extensions.HttpApi.Core
{
    public class ProxyResult : IActionResult
    {
        public ProxyResult(string backendUri)
        {
            BackendUri = backendUri ?? throw new ArgumentNullException(nameof(backendUri));
        }

        public string BackendUri { get; }

        public Action<HttpRequestMessage> BeforeSend { get; set; }

        public Action<HttpResponseMessage> AfterSend { get; set; }

        private static readonly ProxyResultExecutor s_executor = new();

        public Task ExecuteResultAsync(ActionContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return s_executor.ExecuteAsync(context, this);
        }
    }
}
