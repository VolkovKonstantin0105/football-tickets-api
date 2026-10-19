using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace FootballTickets.Web.Services;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly ILocalStorageService _storage;
    public AuthTokenHandler(ILocalStorageService storage) => _storage = storage;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var token = await _storage.GetItemAsStringAsync(JwtAuthStateProvider.TokenKey, ct);
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, ct);
    }
}
