using FootballTickets.Application.Common;
using Microsoft.AspNetCore.Http;

namespace FootballTickets.Api.Services;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;
    public CurrentUser(IHttpContextAccessor http) => _http = http;

    public Guid UserId
    {
        get
        {
            var sub = _http.HttpContext?.User.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var id)
                ? id
                : throw new UnauthorizedException("Not authenticated");
        }
    }
}
