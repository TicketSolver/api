using TicketSolver.Domain.Services;
using TicketSolver.Domain.Services.Interfaces;
using TicketSolver.Domain.Services.Tenant;
using TicketSolver.Domain.Services.Tenant.Interfaces;
using TicketSolver.Domain.Services.Ticket.Interfaces;
using TicketSolver.Domain.Services.User;
using TicketSolver.Domain.Services.User.Interfaces;

namespace TicketSolver.Api.Main;

public static class ConfigureServices
{
    public static void Setup(IServiceCollection services)
    {
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IUsersService, UsersService>();
        services.AddTransient<ITenantsService, TenantsService>();
        services.AddTransient<ITicketsService, TicketsService>();
    }
}