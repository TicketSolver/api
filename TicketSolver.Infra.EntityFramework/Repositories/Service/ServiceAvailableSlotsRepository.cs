using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Persistence.Tables.Service;
using TicketSolver.Domain.Repositories.Service;
using TicketSolver.Infra.EntityFramework.Persistence;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts.Interfaces;

namespace TicketSolver.Infra.EntityFramework.Repositories.Service;

public class ServiceAvailableSlotsRepository(IEfContext context) : EFRepositoryBase<ServiceAvailableSlots>(context), IServiceAvailableSlotsRepository
{
    public async Task<IEnumerable<ServiceAvailableSlots>> GetByServiceRequestIdAsync(int serviceRequestId)
    {
        return await context.ServiceAvailableSlots
            .Where(s => s.ServiceRequestId == serviceRequestId)
            .ToListAsync();
    }

    public async Task<bool> AddSlotsAsync(List<ServiceAvailableSlots> slots)
    {
        await context.ServiceAvailableSlots.AddRangeAsync(slots);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SelectSlotAsync(int slotId)
    {
        var slot = await context.ServiceAvailableSlots.FindAsync(slotId);
        if (slot == null) return false;

        // Limpar outras seleções do mesmo serviço
        var otherSlots = await context.ServiceAvailableSlots
            .Where(s => s.ServiceRequestId == slot.ServiceRequestId && s.Id != slotId)
            .ToListAsync();

        foreach (var otherSlot in otherSlots)
        {
            otherSlot.IsSelected = false;
        }

        slot.IsSelected = true;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ClearSlotsAsync(int serviceRequestId)
    {
        var slots = await context.ServiceAvailableSlots
            .Where(s => s.ServiceRequestId == serviceRequestId)
            .ToListAsync();

        context.ServiceAvailableSlots.RemoveRange(slots);
        await context.SaveChangesAsync();
        return true;
    }
}