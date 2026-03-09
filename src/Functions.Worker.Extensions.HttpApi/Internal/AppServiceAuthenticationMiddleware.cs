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
        var httpContext = context.GetHttpContext();

        if (httpContext is not null)
        {
            httpContext.User = ParsePrincipal(httpContext.Request);
        }

        await next(context);
    }

    private static ClaimsPrincipal ParsePrincipal(HttpRequest req)
    {
        if (!req.Headers.TryGetValue("x-ms-client-principal", out var header) || string.IsNullOrWhiteSpace(header))
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        using var jsonDocument = JsonDocument.Parse(Convert.FromBase64String(header.ToString()));

        if (jsonDocument.RootElement.TryGetProperty("identityProvider", out _))
        {
            var principal = jsonDocument.Deserialize<StaticWebAppClientPrincipal>()
                            ?? throw new InvalidOperationException("The static web app client principal payload is invalid.");

            var identity = new ClaimsIdentity(principal.IdentityProvider);

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
            identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));

            foreach (var role in principal.UserRoles)
            {
                if (!string.Equals(role, "anonymous", StringComparison.OrdinalIgnoreCase))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }

            return new ClaimsPrincipal(identity);
        }
        else
        {
            var principal = jsonDocument.Deserialize<AppServiceClientPrincipal>()
                            ?? throw new InvalidOperationException("The app service client principal payload is invalid.");

            var identity = new ClaimsIdentity(principal.IdentityProvider, principal.NameClaimType, principal.RoleClaimType);

            foreach (var claim in principal.Claims)
            {
                identity.AddClaim(new Claim(claim.Type, claim.Value));
            }

            return new ClaimsPrincipal(identity);
        }
    }

    private sealed record class ClientPrincipalClaim
    {
        [JsonPropertyName("typ")]
        public string Type { get; init; } = string.Empty;
        [JsonPropertyName("val")]
        public string Value { get; init; } = string.Empty;
    }

    private sealed record class AppServiceClientPrincipal
    {
        [JsonPropertyName("auth_typ")]
        public string IdentityProvider { get; init; } = string.Empty;

        [JsonPropertyName("name_typ")]
        public string NameClaimType { get; init; } = string.Empty;

        [JsonPropertyName("role_typ")]
        public string RoleClaimType { get; init; } = string.Empty;

        [JsonPropertyName("claims")]
        public IEnumerable<ClientPrincipalClaim> Claims { get; init; } = [];
    }

    private sealed record class StaticWebAppClientPrincipal
    {
        [JsonPropertyName("identityProvider")]
        public string IdentityProvider { get; init; } = string.Empty;

        [JsonPropertyName("userId")]
        public string UserId { get; init; } = string.Empty;

        [JsonPropertyName("userDetails")]
        public string UserDetails { get; init; } = string.Empty;

        [JsonPropertyName("userRoles")]
        public IEnumerable<string> UserRoles { get; init; } = [];
    }
}
