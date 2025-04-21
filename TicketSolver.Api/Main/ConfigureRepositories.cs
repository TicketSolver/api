using Microsoft.Extensions.DependencyInjection;
using TicketSolver.Domain.Repositories.Tenant;
using TicketSolver.Domain.Repositories.Tenant.Interfaces;
using TicketSolver.Domain.Repositories.User;
using TicketSolver.Domain.Repositories.User.Interfaces;
using TicketSolver.Domain.Services;
using TicketSolver.Domain.Services.Interfaces;
using TicketSolver.Domain.Services.User;

namespace TicketSolver.Api.Main;

public static class ConfigureRepositories
{
    public static void Setup(IServiceCollection services)
    {
        services.AddTransient<ITenantsRepository, TenantsRepository>();
        services.AddTransient<IUsersRepository, UsersReporitory>();
    }
}