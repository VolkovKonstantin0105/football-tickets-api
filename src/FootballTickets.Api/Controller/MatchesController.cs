using FootballTickets.Application.Matches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballTickets.Api.Controllers;

[ApiController]
[Route("api/matches")]
public class MatchesController : ControllerBase
{
    private readonly IMatchService _matches;
    public MatchesController(IMatchService matches) => _matches = matches;

    [HttpGet]
    public async Task<ActionResult<PagedResult<MatchDto>>> GetList(
        [FromQuery] MatchFilter filter, CancellationToken ct)
        => Ok(await _matches.GetListAsync(filter, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MatchDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await _matches.GetByIdAsync(id, ct));

    [HttpGet("{id:guid}/seats")]
    public async Task<ActionResult<List<SectorSeatsDto>>> GetSeats(Guid id, CancellationToken ct)
        => Ok(await _matches.GetSeatsAsync(id, ct));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<MatchDto>> Create(CreateMatchRequest request, CancellationToken ct)
    {
        var created = await _matches.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _matches.DeleteAsync(id, ct);
        return NoContent();
    }
}
