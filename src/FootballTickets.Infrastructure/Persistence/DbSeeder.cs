using FootballTickets.Application.Common;
using FootballTickets.Domain.Entities;
using FootballTickets.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FootballTickets.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IPasswordService passwords, bool includeDemoUsers)
    {
        if (await db.Teams.AnyAsync()) return;

        var teams = new[]
        {
            new Team { Id = Guid.NewGuid(), Name = "Динамо Минск", City = "Минск" },
            new Team { Id = Guid.NewGuid(), Name = "БАТЭ Борисов", City = "Борисов" },
            new Team { Id = Guid.NewGuid(), Name = "Шахтер Солигорск", City = "Солигорск" },
            new Team { Id = Guid.NewGuid(), Name = "Динамо Брест", City = "Брест" },
        };
        db.Teams.AddRange(teams);

        var stadium = new Stadium { Id = Guid.NewGuid(), Name = "Стадион Динамо", City = "Минск" };
        var sectorDefs = new[]
        {
            (Name: "VIP", Multiplier: 2.0m, Rows: 3, PerRow: 10),
            (Name: "A", Multiplier: 1.5m, Rows: 5, PerRow: 12),
            (Name: "B", Multiplier: 1.0m, Rows: 8, PerRow: 15),
        };
        foreach (var def in sectorDefs)
        {
            var sector = new Sector
            {
                Id = Guid.NewGuid(),
                Name = def.Name,
                PriceMultiplier = def.Multiplier
            };
            for (var row = 1; row <= def.Rows; row++)
                for (var number = 1; number <= def.PerRow; number++)
                    sector.Seats.Add(new Seat { Id = Guid.NewGuid(), Row = row, Number = number });
            stadium.Sectors.Add(sector);
        }
        db.Stadiums.Add(stadium);

        db.Matches.AddRange(
            new Match
            {
                Id = Guid.NewGuid(),
                HomeTeamId = teams[0].Id,
                AwayTeamId = teams[1].Id,
                StadiumId = stadium.Id,
                StartsAt = DateTime.UtcNow.AddDays(7),
                BasePrice = 20m
            },
            new Match
            {
                Id = Guid.NewGuid(),
                HomeTeamId = teams[0].Id,
                AwayTeamId = teams[2].Id,
                StadiumId = stadium.Id,
                StartsAt = DateTime.UtcNow.AddDays(14),
                BasePrice = 25m
            },
            new Match
            {
                Id = Guid.NewGuid(),
                HomeTeamId = teams[3].Id,
                AwayTeamId = teams[0].Id,
                StadiumId = stadium.Id,
                StartsAt = DateTime.UtcNow.AddDays(21),
                BasePrice = 15m
            });

        if (includeDemoUsers)
        {
            db.Users.AddRange(
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@tickets.local",
                    FullName = "Admin",
                    Role = UserRole.Admin,
                    PasswordHash = passwords.Hash("Admin12345!"),
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "user@tickets.local",
                    FullName = "Test User",
                    Role = UserRole.User,
                    PasswordHash = passwords.Hash("User12345!"),
                    CreatedAt = DateTime.UtcNow
                });
        }

        await db.SaveChangesAsync();
    }

    public static async Task EnsureAdminAsync(
        AppDbContext db, IPasswordService passwords, string email, string password)
    {
        var normalized = email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == normalized)) return;

        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = normalized,
            FullName = "Administrator",
            Role = UserRole.Admin,
            PasswordHash = passwords.Hash(password),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}