using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Models;
using TicketSolver.Domain.Persistence.Tables.Service;
using TicketSolver.Domain.Repositories.Service;
using TicketSolver.Infra.EntityFramework.Persistence;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts.Interfaces;

namespace TicketSolver.Infra.EntityFramework.Repositories.Service;

public class ServiceRequestRepository(IEfContext context) : EFRepositoryBase<ServiceRequest>(context), IServiceRequestRepository
{
    public async Task<ServiceRequest?> GetByIdAsync(int id)
    {
        return await context.ServiceRequests
            .Include(s => s.Ticket)
            .Include(s => s.RequestedBy)
            .Include(s => s.AssignedTech)
            .Include(s => s.Address)
            .Include(s => s.AvailableSlots)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<ServiceRequest?> GetByTicketIdAsync(int ticketId)
    {
        return await context.ServiceRequests
            .Include(s => s.AvailableSlots)
            .Include(s => s.Address)
            .FirstOrDefaultAsync(s => s.TicketId == ticketId);
    }

    public async Task<ServiceRequest> AddAsync(ServiceRequest service)
    {
        context.ServiceRequests.Add(service);
        await context.SaveChangesAsync();
        return service;
    }

    public async Task<ServiceRequest?> UpdateAsync(ServiceRequest service)
    {
        context.Entry(service).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return service;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var service = await context.ServiceRequests.FindAsync(id);
        if (service == null) return false;

        context.ServiceRequests.Remove(service);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ServiceRequest>> GetByTechnicianAsync(string techId)
    {
        return await context.ServiceRequests
            .Include(s => s.Ticket)
            .Include(s => s.RequestedBy)
            .Include(s => s.Address)
            .Where(s => s.AssignedTechId == techId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ServiceRequest>> GetByUserAsync(string userId)
    {
        return await context.ServiceRequests
            .Include(s => s.Ticket)
            .Include(s => s.AssignedTech)
            .Include(s => s.Address)
            .Where(s => s.RequestedById == userId)
            .ToListAsync();
    }

    public async Task<PaginatedResponse<ServiceRequest>> GetAllAsync(PaginatedQuery query)
    {
        var queryable = context.ServiceRequests
            .Include(s => s.Ticket)
            .Include(s => s.RequestedBy)
            .Include(s => s.AssignedTech)
            .Include(s => s.Address);

        return await ToPaginatedResult(default, queryable, query);
    }
}
