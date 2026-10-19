using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace FootballTickets.Web.Services;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    public const string TokenKey = "authToken";
    private readonly ILocalStorageService _storage;

    public JwtAuthStateProvider(ILocalStorageService storage) => _storage = storage;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _storage.GetItemAsStringAsync(TokenKey);
        if (string.IsNullOrWhiteSpace(token)) return Anonymous();

        try
        {
            var claims = ParseClaims(token).ToList();
            var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            if (exp is not null &&
                DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)) < DateTimeOffset.UtcNow)
            {
                await _storage.RemoveItemAsync(TokenKey);
                return Anonymous();
            }

            var identity = new ClaimsIdentity(claims, "jwt", nameType: "email", roleType: "role");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            await _storage.RemoveItemAsync(TokenKey);
            return Anonymous();
        }
    }

    public void NotifyChanged() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    private static AuthenticationState Anonymous() =>
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private static IEnumerable<Claim> ParseClaims(string jwt)
    {
        var payload = jwt.Split('.')[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
        var values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;
        return values.Select(kv => new Claim(kv.Key, kv.Value.ToString()));
    }
}
