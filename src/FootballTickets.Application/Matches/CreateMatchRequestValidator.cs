using FluentValidation;
namespace FootballTickets.Application.Matches;

public class CreateMatchRequestValidator : AbstractValidator<CreateMatchRequest>
{
    public CreateMatchRequestValidator()
    {
        RuleFor(x => x.HomeTeamId).NotEmpty();
        RuleFor(x => x.AwayTeamId).NotEmpty().NotEqual(x => x.HomeTeamId)
            .WithMessage("Teams must be different");
        RuleFor(x => x.StadiumId).NotEmpty();
        RuleFor(x => x.StartsAt).Must(d => d.ToUniversalTime() > DateTime.UtcNow)
            .WithMessage("Match must start in the future");
        RuleFor(x => x.BasePrice).GreaterThan(0);
    }
}
