using System;

namespace Azure.WebJobs.Extensions.HttpApi
{
    internal static class FunctionEnvironment
    {
        public static bool IsAvailable => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));

        public static string RootPath => IsAvailable ? Environment.ExpandEnvironmentVariables(@"%HOME%\site\wwwroot") : Environment.CurrentDirectory;
    }
}
