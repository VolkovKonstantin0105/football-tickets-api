using FootballTickets.Application.Common;
using FootballTickets.Application.Matches;
using Microsoft.EntityFrameworkCore;

namespace FootballTickets.Application.Catalog;

public record StadiumDto(Guid Id, string Name, string City);

public interface ICatalogService
{
    Task<List<TeamDto>> GetTeamsAsync(CancellationToken ct);
    Task<List<StadiumDto>> GetStadiumsAsync(CancellationToken ct);
}

public class CatalogService : ICatalogService
{
    private readonly IAppDbContext _db;
    public CatalogService(IAppDbContext db) => _db = db;

    public Task<List<TeamDto>> GetTeamsAsync(CancellationToken ct) =>
        _db.Teams.AsNoTracking().OrderBy(t => t.Name)
            .Select(t => new TeamDto(t.Id, t.Name, t.City)).ToListAsync(ct);

    public Task<List<StadiumDto>> GetStadiumsAsync(CancellationToken ct) =>
        _db.Stadiums.AsNoTracking().OrderBy(s => s.Name)
            .Select(s => new StadiumDto(s.Id, s.Name, s.City)).ToListAsync(ct);
}
