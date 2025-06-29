using TicketSolver.Application.Models.Service;
using TicketSolver.Domain.Models;
namespace TicketSolver.Application.Services.Service.Interfaces;

public interface IServiceRequestService
{
    Task<ServiceResponseDTO> RequestServiceAsync(ServiceRequestDTO request, CancellationToken cancellationToken);
    Task<ServiceResponseDTO?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<ServiceResponseDTO?> GetByTicketIdAsync(int ticketId, CancellationToken cancellationToken);
    Task<bool> OfferSlotsAsync(ServiceSlotsDTO slots, string techId, CancellationToken cancellationToken);
    Task<bool> SelectSlotAsync(SelectSlotDTO selection, string userId, CancellationToken cancellationToken);
    Task<bool> StartServiceAsync(int serviceId, string techId, CancellationToken cancellationToken);
    Task<bool> CompleteServiceAsync(ServiceReportDTO report, string techId, CancellationToken cancellationToken);
    Task<bool> CancelServiceAsync(int serviceId, string userId, CancellationToken cancellationToken);
    Task<IEnumerable<ServiceResponseDTO>> GetByTechnicianAsync(string techId, CancellationToken cancellationToken);
    Task<IEnumerable<ServiceResponseDTO>> GetByUserAsync(string userId, CancellationToken cancellationToken);
    Task<PaginatedResponse<ServiceResponseDTO>> GetAllAsync(PaginatedQuery query, CancellationToken cancellationToken);
}
