using FootballTickets.Application.Catalog;
using FootballTickets.Application.Matches;
using Microsoft.AspNetCore.Mvc;

namespace FootballTickets.Api.Controllers;

[ApiController]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalog;
    public CatalogController(ICatalogService catalog) => _catalog = catalog;

    [HttpGet("api/teams")]
    public async Task<ActionResult<List<TeamDto>>> GetTeams(CancellationToken ct)
        => Ok(await _catalog.GetTeamsAsync(ct));

    [HttpGet("api/stadiums")]
    public async Task<ActionResult<List<StadiumDto>>> GetStadiums(CancellationToken ct)
        => Ok(await _catalog.GetStadiumsAsync(ct));
}
