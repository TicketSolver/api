using TicketSolver.Domain.Models;
using TicketSolver.Domain.Persistence.Tables.Ticket;
namespace TicketSolver.Domain.Repositories.admin;

public interface ITenantTicketsRepository

{
    IQueryable<Tickets> GetRecentTicketsAsync(
        int tenantId, int page, int pageSize, CancellationToken ct);
}
