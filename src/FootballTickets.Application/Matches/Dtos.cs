namespace FootballTickets.Application.Matches;
public record TeamDto(Guid Id, string Name, string City);
public record MatchDto(Guid Id, TeamDto HomeTeam, TeamDto AwayTeam, string Stadium,
    DateTime StartsAt, decimal BasePrice, string Status);
public record MatchFilter(DateTime? From, DateTime? To, Guid? TeamId, int Page = 1, int PageSize = 20);
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
public record CreateMatchRequest(Guid HomeTeamId, Guid AwayTeamId, Guid StadiumId,
    DateTime StartsAt, decimal BasePrice);
public record SeatDto(Guid Id, int Row, int Number, decimal Price, bool IsTaken);
public record SectorSeatsDto(Guid SectorId, string Sector, IReadOnlyList<SeatDto> Seats);
