using System.Net.Http.Json;
using Blazored.LocalStorage;
using FootballTickets.Web.Models;

namespace FootballTickets.Web.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _storage;
    private readonly JwtAuthStateProvider _provider;

    public AuthService(HttpClient http, ILocalStorageService storage, JwtAuthStateProvider provider)
    {
        _http = http; _storage = storage; _provider = provider;
    }

    public Task<string?> LoginAsync(string email, string password) =>
        SendAsync("/api/auth/login", new LoginRequest(email, password));

    public Task<string?> RegisterAsync(string email, string password, string fullName) =>
        SendAsync("/api/auth/register", new RegisterRequest(email, password, fullName));

    public async Task LogoutAsync()
    {
        await _storage.RemoveItemAsync(JwtAuthStateProvider.TokenKey);
        _provider.NotifyChanged();
    }

    private async Task<string?> SendAsync<T>(string url, T body)
    {
        var response = await _http.PostAsJsonAsync(url, body);
        if (!response.IsSuccessStatusCode) return await ApiClient.ReadErrorAsync(response);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        await _storage.SetItemAsStringAsync(JwtAuthStateProvider.TokenKey, auth!.Token);
        _provider.NotifyChanged();
        return null;
    }
}
