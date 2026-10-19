using System.Net.Http.Json;
using FootballTickets.Web.Models;

namespace FootballTickets.Web.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    public ApiClient(HttpClient http) => _http = http;

    public Task<PagedResult<MatchDto>?> GetMatchesAsync(int page, Guid? teamId)
    {
        var from = Uri.EscapeDataString(DateTime.UtcNow.ToString("o"));
        var url = $"/api/matches?page={page}&pageSize=6&from={from}";
        if (teamId is not null) url += $"&teamId={teamId}";
        return _http.GetFromJsonAsync<PagedResult<MatchDto>>(url);
    }

    public Task<MatchDto?> GetMatchAsync(Guid id) =>
        _http.GetFromJsonAsync<MatchDto>($"/api/matches/{id}");

    public Task<List<SectorSeatsDto>?> GetSeatsAsync(Guid matchId) =>
        _http.GetFromJsonAsync<List<SectorSeatsDto>>($"/api/matches/{matchId}/seats");

    public Task<List<TeamDto>?> GetTeamsAsync() =>
        _http.GetFromJsonAsync<List<TeamDto>>("/api/teams");

    public Task<List<StadiumDto>?> GetStadiumsAsync() =>
        _http.GetFromJsonAsync<List<StadiumDto>>("/api/stadiums");

    public Task<List<OrderDto>?> GetMyOrdersAsync() =>
        _http.GetFromJsonAsync<List<OrderDto>>("/api/orders/my");

    public async Task<string?> CreateOrderAsync(Guid matchId, List<Guid> seatIds) =>
        await ToErrorAsync(await _http.PostAsJsonAsync("/api/orders", new CreateOrderRequest(matchId, seatIds)));

    public async Task<string?> CancelOrderAsync(Guid orderId) =>
        await ToErrorAsync(await _http.PostAsync($"/api/orders/{orderId}/cancel", null));

    public async Task<string?> CreateMatchAsync(CreateMatchRequest request) =>
        await ToErrorAsync(await _http.PostAsJsonAsync("/api/matches", request));

    public async Task<string?> DeleteMatchAsync(Guid id) =>
        await ToErrorAsync(await _http.DeleteAsync($"/api/matches/{id}"));

    private static async Task<string?> ToErrorAsync(HttpResponseMessage response) =>
        response.IsSuccessStatusCode ? null : await ReadErrorAsync(response);

    private record Problem(string? Title, string? Detail);

    public static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<Problem>();
            if (!string.IsNullOrWhiteSpace(problem?.Detail)) return problem.Detail;
            if (!string.IsNullOrWhiteSpace(problem?.Title)) return problem.Title;
        }
        catch { /* тело не JSON, покажем общий текст ниже */ }

        return (int)response.StatusCode switch
        {
            401 => "Нужно войти в аккаунт",
            403 => "Недостаточно прав",
            429 => "Слишком много запросов, подождите минуту",
            _ => $"Ошибка сервера ({(int)response.StatusCode})"
        };
    }
}
