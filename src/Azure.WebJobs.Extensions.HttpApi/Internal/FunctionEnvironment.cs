using System;

namespace Azure.WebJobs.Extensions.HttpApi.Internal;

internal static class FunctionAppEnvironment
{
    public static bool IsAvailable => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));

    public static string RootPath => IsAvailable ? Environment.ExpandEnvironmentVariables("%HOME%/site/wwwroot") : Environment.CurrentDirectory;
}
