using TicketSolver.Domain.Persistence.Tables.Service;
using TicketSolver.Domain.Repositories.Service;
using TicketSolver.Infra.EntityFramework.Persistence;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts.Interfaces;

namespace TicketSolver.Infra.EntityFramework.Repositories.Service;

public class ServiceAddressRepository(IEfContext context) : EFRepositoryBase<ServiceAddress>(context), IServiceAddressRepository
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