using TicketSolver.Domain.Persistence.Tables.Service;

namespace TicketSolver.Domain.Repositories.Service;

public interface IServiceAvailableSlotsRepository : IRepositoryBase<ServiceAvailableSlots>
{
    Task<IEnumerable<ServiceAvailableSlots>> GetByServiceRequestIdAsync(int serviceRequestId);
    Task<bool> AddSlotsAsync(List<ServiceAvailableSlots> slots);
    Task<bool> SelectSlotAsync(int slotId);
    Task<bool> ClearSlotsAsync(int serviceRequestId);
}