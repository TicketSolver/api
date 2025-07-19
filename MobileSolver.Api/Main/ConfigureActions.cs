using MobileSolver.Application.Actions;
using MobileSolver.Domain.Persistence.Entities;
using TicketSolver.Application.Actions.Ticket.Interfaces;

namespace MobileSolver.Api.Main;

public static class ConfigureActions
{
    public static void AddActions(this IServiceCollection services)
    {
        services.AddTransient<ICreateTicketAction<MobileTickets>, CreateTicketAction>();
    }
}