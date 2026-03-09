namespace Azure.WebJobs.Extensions.HttpApi.Internal;

internal static class FunctionAppEnvironment
{
    private const string WebsiteSiteNameVariableName = "WEBSITE_SITE_NAME";
    private const string WebsiteAuthEnabledVariableName = "WEBSITE_AUTH_ENABLED";

    private static readonly bool s_isRunningOnAzure = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(WebsiteSiteNameVariableName));
    private static readonly bool s_isAuthenticationEnabled = bool.TryParse(Environment.GetEnvironmentVariable(WebsiteAuthEnabledVariableName), out var result) && result;
    private static readonly string s_rootPath = s_isRunningOnAzure ? Environment.ExpandEnvironmentVariables("%HOME%/site/wwwroot") : Environment.CurrentDirectory;

    public static bool IsRunningOnAzure => s_isRunningOnAzure;

    public static bool IsAuthenticationEnabled => s_isAuthenticationEnabled;

    public static string RootPath => s_rootPath;
}
