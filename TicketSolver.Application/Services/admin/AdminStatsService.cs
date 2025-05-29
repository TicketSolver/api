using Microsoft.EntityFrameworkCore;
using TicketSolver.Application.Models.admin;
using TicketSolver.Application.Services.admin.Interfaces;
using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Models;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.admin;
using TicketSolver.Domain.Repositories.Tenant;

namespace TicketSolver.Application.Services.admin;

public class AdminStatsService(
    ITenantsRepository tenantRepo,
    IAdminStatsRepository statsRepo,
    ITenantTicketsRepository ticketsRepo)
    : IAdminStatsService
{
    public async Task<TenantOverviewStatsDto> GetOverviewStatsAsync(
        int tenantid, CancellationToken ct)
    {
        var total = await statsRepo.GetTotalTicketsAsync(tenantid, ct);
        var active = await statsRepo.GetActiveTicketsAsync(tenantid, ct);
        var avg = await statsRepo.GetAverageResolutionTimeAsync(tenantid, ct);
        var techs = await statsRepo.GetActiveTechniciansAsync(tenantid, ct);

        return new TenantOverviewStatsDto
        {
            TotalTickets = total,
            ActiveTickets = active,
            AvgResolutionTime = avg.HasValue
                ? $"{(int)avg.Value.TotalHours}h {avg.Value.Minutes}min"
                : "—",
            ActiveTechnicians = techs
        };
    }

    public async Task<PaginatedResponse<RecentTicketDto>> GetRecentTicketsAsync(int tenantId, int page, int pageSize,
        CancellationToken ct)
    {
        var query = ticketsRepo.GetRecentTicketsAsync(tenantId, page, pageSize, ct);
        var total = query.CountAsync(ct).Result;

        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new RecentTicketDto
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status.ToString(),
                TechnicianName = t.TicketUsers
                    .Where(tu => tu.User.DefUserTypeId == (short)eDefUserTypes.Technician)
                    .Select(tu => tu.User.FullName)
                    .FirstOrDefault()
            })
            .ToListAsync(ct).Result;

        return new PaginatedResponse<RecentTicketDto>(items, page, pageSize, total);
    }


    public Task<PaginatedResponse<Tickets>> GetTicketsAsync(int tenantId, int page, int pageSize,
        CancellationToken ct)
    {
        var query = ticketsRepo.GetRecentTicketsAsync(tenantId, page, pageSize, ct);
        var total = query.CountAsync(ct).Result;

        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct).Result;

        return Task.FromResult(new PaginatedResponse<Tickets>(items, page, pageSize, total));
    }
}