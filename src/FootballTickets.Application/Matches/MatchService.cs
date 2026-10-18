using FluentValidation;
using FootballTickets.Application.Common;
using FootballTickets.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballTickets.Application.Matches;

public interface IMatchService
{
    Task<PagedResult<MatchDto>> GetListAsync(MatchFilter filter, CancellationToken ct);
    Task<MatchDto> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<SectorSeatsDto>> GetSeatsAsync(Guid matchId, CancellationToken ct);
    Task<MatchDto> CreateAsync(CreateMatchRequest request, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}

public class MatchService : IMatchService
{
    private readonly IAppDbContext _db;
    private readonly IValidator<CreateMatchRequest> _createValidator;

    public MatchService(IAppDbContext db, IValidator<CreateMatchRequest> createValidator)
    {
        _db = db; _createValidator = createValidator;
    }

    public async Task<PagedResult<MatchDto>> GetListAsync(MatchFilter f, CancellationToken ct)
    {
        var page = Math.Max(f.Page, 1);
        var size = Math.Clamp(f.PageSize, 1, 100);

        var query = _db.Matches.AsNoTracking().AsQueryable();
        if (f.From is not null) { var from = f.From.Value.ToUtc(); query = query.Where(m => m.StartsAt >= from); }
        if (f.To is not null) { var to = f.To.Value.ToUtc(); query = query.Where(m => m.StartsAt <= to); }
        if (f.TeamId is not null)
            query = query.Where(m => m.HomeTeamId == f.TeamId || m.AwayTeamId == f.TeamId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(m => m.StartsAt)
            .Skip((page - 1) * size).Take(size)
            .Select(m => new MatchDto(m.Id,
                new TeamDto(m.HomeTeam.Id, m.HomeTeam.Name, m.HomeTeam.City),
                new TeamDto(m.AwayTeam.Id, m.AwayTeam.Name, m.AwayTeam.City),
                m.Stadium.Name, m.StartsAt, m.BasePrice, m.Status.ToString()))
            .ToListAsync(ct);

        return new PagedResult<MatchDto>(items, page, size, total);
    }

    public async Task<MatchDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _db.Matches.AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new MatchDto(m.Id,
                new TeamDto(m.HomeTeam.Id, m.HomeTeam.Name, m.HomeTeam.City),
                new TeamDto(m.AwayTeam.Id, m.AwayTeam.Name, m.AwayTeam.City),
                m.Stadium.Name, m.StartsAt, m.BasePrice, m.Status.ToString()))
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("Match not found");
    }

    public async Task<List<SectorSeatsDto>> GetSeatsAsync(Guid matchId, CancellationToken ct)
    {
        var match = await _db.Matches.AsNoTracking().FirstOrDefaultAsync(m => m.Id == matchId, ct)
            ?? throw new NotFoundException("Match not found");

        var taken = (await _db.Tickets.AsNoTracking()
            .Where(t => t.MatchId == matchId && !t.IsCancelled)
            .Select(t => t.SeatId).ToListAsync(ct)).ToHashSet();

        var sectors = await _db.Sectors.AsNoTracking()
            .Where(s => s.StadiumId == match.StadiumId)
            .Include(s => s.Seats)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

        return sectors.Select(s => new SectorSeatsDto(s.Id, s.Name,
            s.Seats.OrderBy(x => x.Row).ThenBy(x => x.Number)
                .Select(x => new SeatDto(x.Id, x.Row, x.Number,
                    Math.Round(match.BasePrice * s.PriceMultiplier, 2), taken.Contains(x.Id)))
                .ToList())).ToList();
    }

    public async Task<MatchDto> CreateAsync(CreateMatchRequest r, CancellationToken ct)
    {
        await _createValidator.ValidateAndThrowAsync(r, ct);

        if (!await _db.Teams.AnyAsync(t => t.Id == r.HomeTeamId, ct))
            throw new NotFoundException("Home team not found");
        if (!await _db.Teams.AnyAsync(t => t.Id == r.AwayTeamId, ct))
            throw new NotFoundException("Away team not found");
        if (!await _db.Stadiums.AnyAsync(s => s.Id == r.StadiumId, ct))
            throw new NotFoundException("Stadium not found");

        var match = new Match
        {
            Id = Guid.NewGuid(),
            HomeTeamId = r.HomeTeamId,
            AwayTeamId = r.AwayTeamId,
            StadiumId = r.StadiumId,
            StartsAt = r.StartsAt.ToUtc(),
            BasePrice = r.BasePrice
        };
        _db.Matches.Add(match);
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(match.Id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var match = await _db.Matches.FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new NotFoundException("Match not found");

        if (await _db.Tickets.AnyAsync(t => t.MatchId == id && !t.IsCancelled, ct))
            throw new ConflictException("Match has active tickets and cannot be deleted");

        _db.Matches.Remove(match);
        await _db.SaveChangesAsync(ct);
    }
}
