namespace Azure.WebJobs.Extensions.HttpApi.Internal;

internal static class FunctionEnvironment
{
    private const string WebsiteSiteNameVariableName = "WEBSITE_SITE_NAME";
    private const string WebsiteAuthEnabledVariableName = "WEBSITE_AUTH_ENABLED";

    public static bool IsRunningOnAzure { get; } = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(WebsiteSiteNameVariableName));

    public static bool IsAuthenticationEnabled { get; } = bool.TryParse(Environment.GetEnvironmentVariable(WebsiteAuthEnabledVariableName), out var result) && result;

    public static string RootPath { get; } = IsRunningOnAzure ? Environment.ExpandEnvironmentVariables("%HOME%/site/wwwroot") : Environment.CurrentDirectory;
}
