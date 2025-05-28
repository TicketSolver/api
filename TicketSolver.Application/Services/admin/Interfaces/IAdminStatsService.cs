using TicketSolver.Application.Models.admin;
using TicketSolver.Domain.Models;
using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace TicketSolver.Application.Services.admin.Interfaces;

public interface IAdminStatsService
{
    Task<TenantOverviewStatsDto> GetOverviewStatsAsync(
        int tenantId, CancellationToken ct);
    Task<PaginatedResponse<RecentTicketDto>> GetRecentTicketsAsync(int tenantId, int page, int pageSize,
        CancellationToken ct);
    Task<PaginatedResponse<Tickets>> GetTicketsAsync(int tenantId, int page, int pageSize, CancellationToken ct);
}