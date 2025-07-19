using Microsoft.EntityFrameworkCore;
using TicketSolver.Application.Models.admin;
using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Repositories.admin;
using TicketSolver.Infra.EntityFramework.Persistence;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts.Interfaces;

namespace TicketSolver.Infra.EntityFramework.Repositories.admin;

public class AdminStatsRepository(IEfContext ctx) : IAdminStatsRepository
{
    public Task<int> GetTotalTicketsAsync(int tenantId, CancellationToken ct)
        => ctx.Tickets
            .Where(t => t.CreatedBy.TenantId == tenantId)
            .CountAsync(ct);

    public Task<int> GetActiveTicketsAsync(int tenantId, CancellationToken ct)
        => ctx.Tickets
            .Where(t => t.CreatedBy.TenantId == tenantId
                        && t.Status != (short)eDefTicketStatus.Closed)
            .CountAsync(ct);

    public async Task<TimeSpan?> GetAverageResolutionTimeAsync(int tenantId, CancellationToken ct)
    {
        var diffs = await ctx.Tickets
            .Where(t => t.CreatedBy.TenantId == tenantId && t.SolvedAt.HasValue)
            .Select(t => EF.Functions.DateDiffSecond(t.CreatedAt, t.SolvedAt!.Value))
            .ToListAsync(ct);

        return diffs.Any()
            ? TimeSpan.FromSeconds(diffs.Average())
            : (TimeSpan?)null;
    }

    public Task<int> GetActiveTechniciansAsync(int tenantId, CancellationToken ct)
        => ctx.TicketUsers
            .Where(tu => tu.Ticket.CreatedBy.TenantId == tenantId)
            .Select(tu => tu.UserId)
            .Distinct()
            .CountAsync(ct);
}