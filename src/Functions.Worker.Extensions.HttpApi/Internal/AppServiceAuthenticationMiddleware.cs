using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Azure.Functions.Worker.Extensions.HttpApi.Internal;

internal class AppServiceAuthenticationMiddleware : IFunctionsWorkerMiddleware
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

    private ClaimsPrincipal ParsePrincipal(HttpRequest req)
    {
        if (!req.Headers.TryGetValue("x-ms-client-principal", out var header))
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        var jsonDocument = JsonDocument.Parse(Convert.FromBase64String(header[0]));

        if (jsonDocument.RootElement.TryGetProperty("identityProvider", out _))
        {
            var principal = jsonDocument.Deserialize<StaticWebAppClientPrincipal>();

            var identity = new ClaimsIdentity(principal.IdentityProvider);

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
            identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
            identity.AddClaims(principal.UserRoles.Except(["anonymous"]).Select(x => new Claim(ClaimTypes.Role, x)));

            return new ClaimsPrincipal(identity);
        }
        else
        {
            var principal = jsonDocument.Deserialize<AppServiceClientPrincipal>();

            var identity = new ClaimsIdentity(principal.IdentityProvider, principal.NameClaimType, principal.RoleClaimType);

            identity.AddClaims(principal.Claims.Select(x => new Claim(x.Type, x.Value)));

            return new ClaimsPrincipal(identity);
        }
    }

    private class ClientPrincipalClaim
    {
        [JsonPropertyName("typ")]
        public string Type { get; init; }
        [JsonPropertyName("val")]
        public string Value { get; init; }
    }

    private class AppServiceClientPrincipal
    {
        [JsonPropertyName("auth_typ")]
        public string IdentityProvider { get; init; }

        [JsonPropertyName("name_typ")]
        public string NameClaimType { get; init; }

        [JsonPropertyName("role_typ")]
        public string RoleClaimType { get; init; }

        [JsonPropertyName("claims")]
        public IEnumerable<ClientPrincipalClaim> Claims { get; init; }
    }

    private class StaticWebAppClientPrincipal
    {
        [JsonPropertyName("identityProvider")]
        public string IdentityProvider { get; init; }

        [JsonPropertyName("userId")]
        public string UserId { get; init; }

        [JsonPropertyName("userDetails")]
        public string UserDetails { get; init; }

        [JsonPropertyName("userRoles")]
        public IEnumerable<string> UserRoles { get; init; }
    }
}
