namespace TicketSolver.Domain.Repositories.admin;

public interface IAdminStatsRepository
{
    Task<int> GetTotalTicketsAsync(int tenantId, CancellationToken ct);
    Task<int> GetActiveTicketsAsync(int tenantId, CancellationToken ct);
    Task<TimeSpan?> GetAverageResolutionTimeAsync(int tenantId, CancellationToken ct);
    Task<int> GetActiveTechniciansAsync(int tenantId, CancellationToken ct);
}