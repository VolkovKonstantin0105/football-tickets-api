namespace FootballTickets.Web.Models;

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password, string FullName);
public record AuthResponse(string Token);

public record TeamDto(Guid Id, string Name, string City);
public record StadiumDto(Guid Id, string Name, string City);

public record MatchDto(Guid Id, TeamDto HomeTeam, TeamDto AwayTeam, string Stadium,
    DateTime StartsAt, decimal BasePrice, string Status);
public record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalCount);
public record CreateMatchRequest(Guid HomeTeamId, Guid AwayTeamId, Guid StadiumId,
    DateTime StartsAt, decimal BasePrice);

public record SeatDto(Guid Id, int Row, int Number, decimal Price, bool IsTaken);
public record SectorSeatsDto(Guid SectorId, string Sector, List<SeatDto> Seats);

public record CreateOrderRequest(Guid MatchId, List<Guid> SeatIds);
public record TicketDto(Guid Id, Guid SeatId, string Sector, int Row, int Number,
    decimal Price, bool IsCancelled);
public record OrderDto(Guid Id, Guid MatchId, string Match, DateTime MatchStartsAt,
    DateTime CreatedAt, string Status, decimal TotalAmount, List<TicketDto> Tickets);
