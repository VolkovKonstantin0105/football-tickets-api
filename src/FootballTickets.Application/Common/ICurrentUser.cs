namespace FootballTickets.Application.Common;

public interface ICurrentUser
{
    Guid UserId { get; }
}
