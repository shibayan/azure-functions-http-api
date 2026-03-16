using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Azure.Functions.Worker.Extensions.HttpApi.Internal;

internal sealed class AppServiceAuthenticationMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var httpContext = context.GetHttpContext();

        if (httpContext is not null)
        {
            httpContext.User = ParsePrincipal(httpContext.Request);
        }

        await next(context);
    }

    private static ClaimsPrincipal ParsePrincipal(HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        static ClaimsPrincipal CreateAnonymousPrincipal() => new(new ClaimsIdentity());

        if (!request.Headers.TryGetValue("x-ms-client-principal", out var header) || string.IsNullOrWhiteSpace(header))
        {
            return CreateAnonymousPrincipal();
        }

        byte[] principalBytes;

        try
        {
            principalBytes = Convert.FromBase64String(header.ToString());
        }
        catch (FormatException)
        {
            return CreateAnonymousPrincipal();
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(principalBytes);

            if (jsonDocument.RootElement.TryGetProperty("identityProvider", out _))
            {
                return ParseStaticWebAppPrincipal(jsonDocument) ?? CreateAnonymousPrincipal();
            }

            return ParseAppServicePrincipal(jsonDocument) ?? CreateAnonymousPrincipal();
        }
        catch (JsonException)
        {
            return CreateAnonymousPrincipal();
        }
        catch (NotSupportedException)
        {
            return CreateAnonymousPrincipal();
        }
    }

    private static ClaimsPrincipal? ParseAppServicePrincipal(JsonDocument jsonDocument)
    {
        var principal = jsonDocument.Deserialize<AppServiceClientPrincipal>();

        if (principal is null)
        {
            return null;
        }

        var identity = new ClaimsIdentity(principal.IdentityProvider, principal.NameClaimType, principal.RoleClaimType);

        foreach (var claim in principal.Claims)
        {
            identity.AddClaim(new Claim(claim.Type, claim.Value));
        }

        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal? ParseStaticWebAppPrincipal(JsonDocument jsonDocument)
    {
        var staticWebAppPrincipal = jsonDocument.Deserialize<StaticWebAppClientPrincipal>();

        if (staticWebAppPrincipal is null)
        {
            return null;
        }

        var staticWebAppIdentity = new ClaimsIdentity(staticWebAppPrincipal.IdentityProvider);

        staticWebAppIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, staticWebAppPrincipal.UserId));
        staticWebAppIdentity.AddClaim(new Claim(ClaimTypes.Name, staticWebAppPrincipal.UserDetails));

        foreach (var role in staticWebAppPrincipal.UserRoles ?? [])
        {
            if (!string.Equals(role, "anonymous", StringComparison.OrdinalIgnoreCase))
            {
                staticWebAppIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }

        return new ClaimsPrincipal(staticWebAppIdentity);
    }

    internal sealed record ClientPrincipalClaim
    {
        [JsonPropertyName("typ")]
        public required string Type { get; init; }
        [JsonPropertyName("val")]
        public required string Value { get; init; }
    }

    internal sealed record AppServiceClientPrincipal
    {
        [JsonPropertyName("auth_typ")]
        public required string IdentityProvider { get; init; }

        [JsonPropertyName("name_typ")]
        public required string NameClaimType { get; init; }

        [JsonPropertyName("role_typ")]
        public string? RoleClaimType { get; init; }

        [JsonPropertyName("claims")]
        public required ClientPrincipalClaim[] Claims { get; init; }
    }

    internal sealed record StaticWebAppClientPrincipal
    {
        [JsonPropertyName("identityProvider")]
        public required string IdentityProvider { get; init; }

        [JsonPropertyName("userId")]
        public required string UserId { get; init; }

        [JsonPropertyName("userDetails")]
        public required string UserDetails { get; init; }

        [JsonPropertyName("userRoles")]
        public string[]? UserRoles { get; init; }
    }
}
