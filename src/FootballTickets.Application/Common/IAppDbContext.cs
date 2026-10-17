using FootballTickets.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballTickets.Application.Common;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Team> Teams { get; }
    DbSet<Stadium> Stadiums { get; }
    DbSet<Sector> Sectors { get; }
    DbSet<Seat> Seats { get; }
    DbSet<Match> Matches { get; }
    DbSet<Order> Orders { get; }
    DbSet<Ticket> Tickets { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
