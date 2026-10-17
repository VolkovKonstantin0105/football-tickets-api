using FootballTickets.Domain.Entities;
namespace FootballTickets.Application.Common;
public interface IJwtTokenGenerator
{
    string Generate(User user);
}

