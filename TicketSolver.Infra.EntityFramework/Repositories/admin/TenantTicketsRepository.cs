using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Models;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.admin;
using TicketSolver.Infra.EntityFramework.Persistence;
using RecentTicketDto = TicketSolver.Application.Models.admin.RecentTicketDto;

namespace TicketSolver.Infra.EntityFramework.Repositories.admin;

public class TenantTicketsRepository(EfContext ctx) : ITenantTicketsRepository
{
    public IQueryable<Tickets> GetRecentTicketsAsync(
        int tenantId, int page, int pageSize, CancellationToken ct)
    {
        var query = ctx.Tickets
            .Include(t => t.TicketUsers).ThenInclude(tu => tu.User)
            .Where(t => t.CreatedBy.TenantId == tenantId)
            .OrderByDescending(t => t.CreatedAt);
        return query;
    }
}