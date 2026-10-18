using FootballTickets.Application.Common;
using FootballTickets.Application.Orders;
using FootballTickets.Domain.Entities;
using FootballTickets.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Match = FootballTickets.Domain.Entities.Match;

namespace FootballTickets.Application.Tests;

public class OrderServiceTests
{
    private static readonly DateTime Now = new(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc);

    private record Fixture(Match Match, Seat VipSeat, Seat StandardSeat, Guid UserId);

    private static AppDbContext CreateDb() => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static async Task<Fixture> SeedAsync(AppDbContext db, DateTime startsAt)
    {
        var home = new Team { Id = Guid.NewGuid(), Name = "Home", City = "A" };
        var away = new Team { Id = Guid.NewGuid(), Name = "Away", City = "B" };
        var stadium = new Stadium { Id = Guid.NewGuid(), Name = "Arena", City = "A" };
        var vip = new Sector { Id = Guid.NewGuid(), Name = "VIP", PriceMultiplier = 2.0m, Stadium = stadium };
        var std = new Sector { Id = Guid.NewGuid(), Name = "B", PriceMultiplier = 1.0m, Stadium = stadium };
        var vipSeat = new Seat { Id = Guid.NewGuid(), Row = 1, Number = 1, Sector = vip };
        var stdSeat = new Seat { Id = Guid.NewGuid(), Row = 1, Number = 1, Sector = std };
        var user = new User { Id = Guid.NewGuid(), Email = "t@t.t", FullName = "T", PasswordHash = "x" };
        var match = new Match
        {
            Id = Guid.NewGuid(),
            HomeTeam = home,
            AwayTeam = away,
            Stadium = stadium,
            StartsAt = startsAt,
            BasePrice = 20m
        };
        db.AddRange(home, away, stadium, vip, std, vipSeat, stdSeat, user, match);
        await db.SaveChangesAsync();
        return new Fixture(match, vipSeat, stdSeat, user.Id);
    }

    private static OrderService CreateSut(AppDbContext db, Guid userId)
    {
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(u => u.UserId).Returns(userId);
        return new OrderService(db, currentUser.Object, new CreateOrderRequestValidator(),
            new FakeTimeProvider(new DateTimeOffset(Now)), NullLogger<OrderService>.Instance);
    }

    [Fact]
    public async Task Create_CalculatesTotalUsingSectorMultipliers()
    {
        await using var db = CreateDb();
        var f = await SeedAsync(db, Now.AddDays(10));
        var sut = CreateSut(db, f.UserId);

        var result = await sut.CreateAsync(
            new CreateOrderRequest(f.Match.Id, new() { f.VipSeat.Id, f.StandardSeat.Id }), default);

        Assert.Equal(60m, result.TotalAmount); // 20 * 2.0 + 20 * 1.0
        Assert.Equal(2, result.Tickets.Count);
    }

    [Fact]
    public async Task Create_Throws_WhenMatchAlreadyStarted()
    {
        await using var db = CreateDb();
        var f = await SeedAsync(db, Now.AddHours(-1));
        var sut = CreateSut(db, f.UserId);

        await Assert.ThrowsAsync<BusinessRuleException>(() => sut.CreateAsync(
            new CreateOrderRequest(f.Match.Id, new() { f.VipSeat.Id }), default));
    }

    [Fact]
    public async Task Create_Throws_WhenSeatIsAlreadyTaken()
    {
        await using var db = CreateDb();
        var f = await SeedAsync(db, Now.AddDays(10));
        var sut = CreateSut(db, f.UserId);
        var request = new CreateOrderRequest(f.Match.Id, new() { f.VipSeat.Id });

        await sut.CreateAsync(request, default);

        await Assert.ThrowsAsync<ConflictException>(() => sut.CreateAsync(request, default));
    }

    [Fact]
    public async Task Cancel_Throws_WhenLessThan24HoursBeforeMatch()
    {
        await using var db = CreateDb();
        var f = await SeedAsync(db, Now.AddHours(12));
        // заказ создаем "в прошлом", когда продажа еще была возможна
        var earlier = new OrderService(db, Mock.Of<ICurrentUser>(u => u.UserId == f.UserId),
            new CreateOrderRequestValidator(),
            new FakeTimeProvider(new DateTimeOffset(Now.AddDays(-5))), NullLogger<OrderService>.Instance);
        var order = await earlier.CreateAsync(
            new CreateOrderRequest(f.Match.Id, new() { f.VipSeat.Id }), default);

        var sut = CreateSut(db, f.UserId);

        await Assert.ThrowsAsync<BusinessRuleException>(() => sut.CancelAsync(order.Id, default));
    }

    [Fact]
    public async Task Cancel_FreesSeats_SoTheyCanBeBoughtAgain()
    {
        await using var db = CreateDb();

        var f = await SeedAsync(db, Now.AddDays(12)); //кладу в переменную базу данных в переменную, виорой пареметр - через сколько матч
        var sut = CreateSut(db, f.UserId); //переменная для работы с id-пользователя
        var request = new CreateOrderRequest(f.Match.Id, new List<Guid>() { f.VipSeat.Id });
        var order = await sut.CreateAsync(request, default);
        await sut.CancelAsync(order.Id, default);
        var second = await sut.CreateAsync(request, default);
        Assert.Equal("Paid", second.Status);
    }

    [Fact]
    public async  Task Cancel_Throws_WhenOrderBelongsToAnotherUser()
    {
        await using var db = CreateDb();
        var f = await SeedAsync(db, Now.AddDays(2));
        var sut = CreateSut(db, f.UserId);
        var request = new CreateOrderRequest(f.Match.Id, new List<Guid>() { f.VipSeat.Id });
        var order = await sut.CreateAsync(request, default);
        var stranger = CreateSut(db, Guid.NewGuid());
        await Assert.ThrowsAsync<NotFoundException>(() => stranger.CancelAsync(order.Id, default));
    }

    [Fact]
    public async Task Create_Throws_WhenMoreThanSixSeats()
    {
        await using var db = CreateDb();
        var f = await SeedAsync(db, Now.AddDays(3));
        var sut = CreateSut(db, f.UserId);
        
        var sevenSeats = Enumerable.Range(0, 7).Select(_ => Guid.NewGuid()).ToList();
        var request = new CreateOrderRequest(f.Match.Id, sevenSeats);
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => sut.CreateAsync(request, default));
    }
}
