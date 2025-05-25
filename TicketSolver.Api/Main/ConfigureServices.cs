using TicketSolver.Application.Services;
using TicketSolver.Application.Services.Interfaces;
using TicketSolver.Application.Services.Tenant;
using TicketSolver.Application.Services.Tenant.Interfaces;
using TicketSolver.Application.Services.Ticket;
using TicketSolver.Application.Services.Ticket.Interfaces;
using TicketSolver.Application.Services.User;
using TicketSolver.Application.Services.User.Interfaces;

namespace TicketSolver.Api.Main;

public static class ConfigureServices
{
    public static void Setup(IServiceCollection services)
    {
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IUsersService, UsersService>();
        services.AddTransient<ITenantsService, TenantsService>();
        services.AddTransient<ITicketsService, TicketsService>();
        services.AddTransient<IAttachmentsService, AttachmentsService>();
    }
}