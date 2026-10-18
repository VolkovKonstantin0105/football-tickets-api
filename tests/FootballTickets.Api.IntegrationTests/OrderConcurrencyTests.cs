using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
//тест для говнок, один получит 201, а дургой 409
namespace FootballTickets.Api.IntegrationTests;

public class OrderConcurrencyTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public OrderConcurrencyTests(ApiFactory factory) => _factory = factory;

    private record AuthDto(string Token);
    private record MatchItem(Guid Id);
    private record MatchesPage(List<MatchItem> Items);
    private record SeatItem(Guid Id, bool IsTaken);
    private record SectorItem(List<SeatItem> Seats);

    [Fact]
    public async Task TwoBuyers_SameSeat_OnlyOneSucceeds()
    {
        var anon = _factory.CreateClient();
        var tokenA = await GetTokenAsync(anon, "/api/auth/login",
            new { email = "user@tickets.local", password = "User12345!" });
        var tokenB = await GetTokenAsync(anon, "/api/auth/register",
            new { email = "second@tickets.local", password = "Second12345!", fullName = "Second" });

        var matches = await anon.GetFromJsonAsync<MatchesPage>("/api/matches");
        var matchId = matches!.Items[0].Id;
        var sectors = await anon.GetFromJsonAsync<List<SectorItem>>($"/api/matches/{matchId}/seats");
        var seatId = sectors![0].Seats.First(s => !s.IsTaken).Id;

        var results = await Task.WhenAll(
            BuyAsync(tokenA, matchId, seatId), BuyAsync(tokenB, matchId, seatId));

        Assert.Equal(1, results.Count(r => r == HttpStatusCode.Created));
        Assert.Equal(1, results.Count(r => r == HttpStatusCode.Conflict));
    }

    private static async Task<string> GetTokenAsync(HttpClient client, string url, object body)
    {
        var response = await client.PostAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthDto>())!.Token;
    }

    private async Task<HttpStatusCode> BuyAsync(string token, Guid matchId, Guid seatId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.PostAsJsonAsync("/api/orders",
            new { matchId, seatIds = new[] { seatId } });
        return response.StatusCode;
    }
}
