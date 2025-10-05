using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Azure.Functions.Worker.Extensions.HttpApi.Internal;

internal class AuhorizationMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpContext = context.GetHttpContext();

        httpContext.User = Parse(httpContext.Request);

        await next(context);
    }

    public static ClaimsPrincipal Parse(HttpRequest req)
    {
        var principal = new ClientPrincipal();

        if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
        {
            var data = header[0];
            var decoded = Convert.FromBase64String(data);
            var json = Encoding.UTF8.GetString(decoded);
            principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        var identity = new ClaimsIdentity(principal.IdentityProvider, principal.NameClaimType, principal.RoleClaimType);
        identity.AddClaims(principal.Claims.Select(c => new Claim(c.Type, c.Value)));

        return new ClaimsPrincipal(identity);
    }

    private class ClientPrincipalClaim
    {
        [JsonPropertyName("typ")]
        public string Type { get; init; }
        [JsonPropertyName("val")]
        public string Value { get; init; }
    }

    private class ClientPrincipal
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
}
