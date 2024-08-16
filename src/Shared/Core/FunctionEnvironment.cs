using System;

namespace Azure.WebJobs.Extensions.HttpApi.Internal;

internal static class FunctionAppEnvironment
{
    public static bool IsRunningOnAzure => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));

    public static bool IsAuthenticationEnabled => bool.TryParse(Environment.GetEnvironmentVariable("WEBSITE_AUTH_ENABLED"), out var result) && result;

    public static string RootPath => IsRunningOnAzure ? Environment.ExpandEnvironmentVariables("%HOME%/site/wwwroot") : Environment.CurrentDirectory;
}
