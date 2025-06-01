using TicketSolver.Domain.Repositories.admin;
using TicketSolver.Domain.Repositories.Chat;
using TicketSolver.Domain.Repositories.Tenant;
using TicketSolver.Domain.Repositories.Ticket;
using TicketSolver.Domain.Repositories.User;
using TicketSolver.Infra.EntityFramework.Repositories.admin;
using TicketSolver.Infra.EntityFramework.Repositories.Chat;
using TicketSolver.Infra.EntityFramework.Repositories.Tenant;
using TicketSolver.Infra.EntityFramework.Repositories.Ticket;
using TicketSolver.Infra.EntityFramework.Repositories.User;

namespace TicketSolver.Api.Main;

public static class ConfigureRepositories
{
    public static void Setup(IServiceCollection services)
    {
        services.AddTransient<IUsersRepository, UsersRepository>();
        services.AddTransient<ITicketsRepository, TicketsRepository>();
        services.AddTransient<ITenantsRepository, TenantsRepository>();
        services.AddTransient<IAttachmentsRepository, AttachmentsRepository>();
        services.AddTransient<IChatRepository, ChatRepository>();
        services.AddTransient<ITicketUsersRepository, TicketUsersRepository>();
        services.AddTransient<IAdminStatsRepository, AdminStatsRepository>();
        services.AddTransient<ITenantTicketsRepository, TenantTicketsRepository>();
    }
}