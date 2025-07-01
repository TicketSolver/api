using TicketSolver.Application.Actions.Ticket;
using TicketSolver.Application.Actions.Ticket.Interfaces;
using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace TicketSolver.Api.Main;

public static class ConfigureActions
{
    public static void AddActions(this IServiceCollection services)
    {
        services.AddTransient<ICreateTicketAction<Tickets>, BaseCreateTicketAction>();
    }
}