using TicketSolver.Domain.Persistence.Tables.Service;
using TicketSolver.Domain.Repositories.Service;
using TicketSolver.Infra.EntityFramework.Persistence;

namespace TicketSolver.Infra.EntityFramework.Repositories.Service;

public class ServiceAddressRepository(EfContext context) : EFRepositoryBase<ServiceAddress>(context), IServiceAddressRepository
{
    public async Task<ServiceAddress> AddAsync(ServiceAddress address)
    {
        context.ServiceAddresses.Add(address);
        await context.SaveChangesAsync();
        return address;
    }

    public async Task<ServiceAddress?> GetByIdAsync(int id)
    {
        return await context.ServiceAddresses.FindAsync(id);
    }
}