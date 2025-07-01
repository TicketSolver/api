using TicketSolver.Domain.Persistence.Tables.Service;

namespace TicketSolver.Domain.Repositories.Service;

public interface IServiceAddressRepository : IRepositoryBase<ServiceAddress>
{
    Task<ServiceAddress> AddAsync(ServiceAddress address);
    Task<ServiceAddress?> GetByIdAsync(int id);
}