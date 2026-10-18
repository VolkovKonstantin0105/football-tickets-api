using FluentValidation;

namespace FootballTickets.Application.Orders;

public record CreateOrderRequest(Guid MatchId, List<Guid> SeatIds);

public record TicketDto(Guid Id, Guid SeatId, string Sector, int Row, int Number,
    decimal Price, bool IsCancelled);

public record OrderDto(Guid Id, Guid MatchId, string Match, DateTime MatchStartsAt,
    DateTime CreatedAt, string Status, decimal TotalAmount, IReadOnlyList<TicketDto> Tickets);

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.SeatIds).NotNull().Must(s => s != null && s.Count is >= 1 and <= 6)
            .WithMessage("An order must contain from 1 to 6 seats");
        RuleFor(x => x.SeatIds).Must(s => s == null || s.Distinct().Count() == s.Count)
            .WithMessage("Seats must not repeat");
    }
}
