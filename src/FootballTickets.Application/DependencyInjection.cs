using FluentValidation;
using FootballTickets.Application.Auth;
using FootballTickets.Application.Catalog;
using FootballTickets.Application.Matches;
using FootballTickets.Application.Orders;
using Microsoft.Extensions.DependencyInjection;

namespace FootballTickets.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMatchService, MatchService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ICatalogService, CatalogService>();
        return services;
    }
}
