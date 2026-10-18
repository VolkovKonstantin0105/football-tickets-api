using FluentValidation;
using FootballTickets.Application.Common;
using FootballTickets.Domain.Entities;
using FootballTickets.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FootballTickets.Application.Orders;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken ct);
    Task<List<OrderDto>> GetMyAsync(CancellationToken ct);
    Task<OrderDto> GetByIdAsync(Guid id, CancellationToken ct);
    Task<OrderDto> CancelAsync(Guid id, CancellationToken ct);
}

public class OrderService : IOrderService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<CreateOrderRequest> _validator;
    private readonly TimeProvider _time;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IAppDbContext db, ICurrentUser currentUser,
        IValidator<CreateOrderRequest> validator, TimeProvider time,
        ILogger<OrderService> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _validator = validator;
        _time = time;
        _logger = logger;
    }

    private DateTime UtcNow => _time.GetUtcNow().UtcDateTime;

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAsync(request, ct);
        var userId = _currentUser.UserId;

        var match = await _db.Matches.FirstOrDefaultAsync(m => m.Id == request.MatchId, ct)
            ?? throw new NotFoundException("Match not found");

        if (match.Status != MatchStatus.Scheduled || match.StartsAt <= UtcNow)
        {
            _logger.LogWarning(
                "Rejected order creation for match {MatchId} by user {UserId}: match is not on sale (status {Status}, starts at {StartsAt:o})",
                match.Id, userId, match.Status, match.StartsAt);
            throw new BusinessRuleException("Tickets for this match are not on sale");
        }

        var seatIds = request.SeatIds;
        var seats = await _db.Seats.Include(s => s.Sector)
            .Where(s => seatIds.Contains(s.Id) && s.Sector.StadiumId == match.StadiumId)
            .ToListAsync(ct);
        if (seats.Count != seatIds.Count)
            throw new NotFoundException("Some seats were not found at this stadium");

        var anyTaken = await _db.Tickets.AnyAsync(
            t => t.MatchId == match.Id && !t.IsCancelled && seatIds.Contains(t.SeatId), ct);
        if (anyTaken)
        {
            _logger.LogWarning(
                "Rejected order creation for match {MatchId} by user {UserId}: some of {SeatCount} seats are already taken",
                match.Id, userId, seatIds.Count);
            throw new ConflictException("Some seats are already taken");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = UtcNow,
            Status = OrderStatus.Paid
        };
        foreach (var seat in seats)
        {
            order.Tickets.Add(new Ticket
            {
                Id = Guid.NewGuid(),
                MatchId = match.Id,
                SeatId = seat.Id,
                Price = Math.Round(match.BasePrice * seat.Sector.PriceMultiplier, 2)
            });
        }
        order.TotalAmount = order.Tickets.Sum(t => t.Price);

        _db.Orders.Add(order);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            // сработал уникальный индекс: другой покупатель успел раньше
            _logger.LogWarning(ex,
                "Concurrent purchase conflict for match {MatchId} by user {UserId}: seats {SeatIds}",
                match.Id, userId, seatIds);
            throw new ConflictException("Some seats were just taken by another buyer");
        }

        _logger.LogInformation(
            "Order {OrderId} created by user {UserId} for match {MatchId}: {TicketCount} tickets, total {TotalAmount}",
            order.Id, userId, match.Id, order.Tickets.Count, order.TotalAmount);

        return await GetByIdAsync(order.Id, ct);
    }

    public async Task<List<OrderDto>> GetMyAsync(CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        var orders = await OrdersWithDetails()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
        return orders.Select(ToDto).ToList();
    }

    public async Task<OrderDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        var order = await OrdersWithDetails()
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId, ct)
            ?? throw new NotFoundException("Order not found");
        return ToDto(order);
    }

    public async Task<OrderDto> CancelAsync(Guid id, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        var order = await _db.Orders.Include(o => o.Tickets)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId, ct)
            ?? throw new NotFoundException("Order not found");

        if (order.Status == OrderStatus.Cancelled)
        {
            _logger.LogWarning(
                "Rejected cancellation of order {OrderId} by user {UserId}: already cancelled",
                order.Id, userId);
            throw new ConflictException("Order is already cancelled");
        }

        var matchId = order.Tickets[0].MatchId;
        var startsAt = await _db.Matches.Where(m => m.Id == matchId)
            .Select(m => m.StartsAt).FirstAsync(ct);
        if (startsAt - UtcNow < TimeSpan.FromHours(24))
        {
            _logger.LogWarning(
                "Rejected cancellation of order {OrderId} by user {UserId}: less than 24h before match {MatchId} (starts at {StartsAt:o})",
                order.Id, userId, matchId, startsAt);
            throw new BusinessRuleException(
                "An order can be cancelled no later than 24 hours before the match");
        }

        order.Status = OrderStatus.Cancelled;
        foreach (var ticket in order.Tickets) ticket.IsCancelled = true;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Order {OrderId} cancelled by user {UserId} (match {MatchId}, {TicketCount} tickets)",
            order.Id, userId, matchId, order.Tickets.Count);

        return await GetByIdAsync(id, ct);
    }

    private IQueryable<Order> OrdersWithDetails() => _db.Orders.AsNoTracking()
        .Include(o => o.Tickets).ThenInclude(t => t.Match).ThenInclude(m => m.HomeTeam)
        .Include(o => o.Tickets).ThenInclude(t => t.Match).ThenInclude(m => m.AwayTeam)
        .Include(o => o.Tickets).ThenInclude(t => t.Seat).ThenInclude(s => s.Sector);

    private static OrderDto ToDto(Order o)
    {
        var match = o.Tickets.First().Match;
        return new OrderDto(o.Id, match.Id,
            $"{match.HomeTeam.Name} - {match.AwayTeam.Name}", match.StartsAt,
            o.CreatedAt, o.Status.ToString(), o.TotalAmount,
            o.Tickets.Select(t => new TicketDto(t.Id, t.SeatId, t.Seat.Sector.Name,
                t.Seat.Row, t.Seat.Number, t.Price, t.IsCancelled)).ToList());
    }
}