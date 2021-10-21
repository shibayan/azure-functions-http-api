﻿using System;

using Azure.WebJobs.Extensions.HttpApi.Routing;

using Microsoft.Azure.WebJobs;

namespace Microsoft.Extensions.Hosting
{
    public static class HttpApiWebJobsBuilderExtensions
    {
        public static IWebJobsBuilder AddHttpApi(this IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddExtension<RoutePrecedenceExtensionConfigProvider>();

            return builder;
        }
    }
}
