using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace TicketSolver.Application.Actions.Users.Interfaces;

public interface INotifyTechcnicianAction<TTickets> : INotifyUserAction<TTickets> where TTickets : Tickets
{
}