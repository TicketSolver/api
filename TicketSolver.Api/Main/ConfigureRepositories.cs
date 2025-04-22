using TicketSolver.Domain.Repositories.Tenant;
using TicketSolver.Domain.Repositories.Tenant.Interfaces;
using TicketSolver.Domain.Repositories.Ticket;
using TicketSolver.Domain.Repositories.Ticket.Interfaces;
using TicketSolver.Domain.Repositories.User;
using TicketSolver.Domain.Repositories.User.Interfaces;

namespace TicketSolver.Api.Main;

public static class ConfigureRepositories
{
    public static void Setup(IServiceCollection services)
    {
        services.AddTransient<ITenantsRepository, TenantsRepository>();
        services.AddTransient<IUsersRepository, UsersRepository>();
        services.AddTransient<ITicketsRepository, TicketsRepository>();
        
    }
}