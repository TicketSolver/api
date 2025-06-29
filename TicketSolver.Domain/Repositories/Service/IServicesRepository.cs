using TicketSolver.Domain.Models;
using TicketSolver.Domain.Persistence.Tables.Service;

namespace TicketSolver.Domain.Repositories.Service;

public interface IServiceRequestRepository : IRepositoryBase<ServiceRequest>
{
    Task<ServiceRequest?> GetByIdAsync(int id);
    Task<ServiceRequest?> GetByTicketIdAsync(int ticketId);
    Task<ServiceRequest> AddAsync(ServiceRequest service);
    Task<ServiceRequest?> UpdateAsync(ServiceRequest service);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<ServiceRequest>> GetByTechnicianAsync(string techId);
    Task<IEnumerable<ServiceRequest>> GetByUserAsync(string userId);
    Task<PaginatedResponse<ServiceRequest>> GetAllAsync(PaginatedQuery query);
}