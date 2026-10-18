using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using FootballTickets.Application.Auth;
using FootballTickets.Application.Matches;

namespace FootballTickets.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMatchService, MatchService>();
        // IOrderService добавим на шаге 13
        return services;
    }
}
