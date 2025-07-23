using Microsoft.EntityFrameworkCore;
using TicketSolver.Application.Exceptions.Service;
using TicketSolver.Application.Exceptions.Ticket;
using TicketSolver.Application.Models.Service;
using TicketSolver.Application.Services.Service.Interfaces;
using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Models;
using TicketSolver.Domain.Persistence.Tables.Service;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Service;
using TicketSolver.Domain.Repositories.Ticket;

namespace TicketSolver.Application.Services.Service;

public class ServiceRequestService(
    IServiceRequestRepository serviceRequestRepository,
    IServiceAvailableSlotsRepository slotsRepository,
    IServiceAddressRepository addressRepository,
    ITicketsRepository<Tickets> ticketsRepository
) : IServiceRequestService
{
    public async Task<ServiceResponseDTO> RequestServiceAsync(ServiceRequestDTO request, CancellationToken cancellationToken)
    {
        var ticketExists = await ticketsRepository.ExistsAsync(request.TicketId, cancellationToken);
        if (!ticketExists)
            throw new TicketNotFoundException();
        var existingService = await serviceRequestRepository.GetByTicketIdAsync(request.TicketId);
        if (existingService != null)
            throw new ServiceException("Já existe um atendimento solicitado para este ticket.");
        var address = new ServiceAddress
        {
            Street = request.Address.Street,
            City = request.Address.City,
            State = request.Address.State,
            ZipCode = request.Address.ZipCode,
            Country = request.Address.Country,
            Complement = request.Address.Complement,
            Reference = request.Address.Reference,
            CreatedAt = DateTime.Now
        };

        address = await addressRepository.AddAsync(address);
        var service = new ServiceRequest
        {
            TicketId = request.TicketId,
            RequestedById = request.RequestedById,
            Status = (short)eDefServiceStatus.Pending,
            AddressId = address.Id,
            CreatedAt = DateTime.Now
        };

        service = await serviceRequestRepository.AddAsync(service);
        return await MapToResponseDTO(service);
    }

    public async Task<ServiceResponseDTO?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var service = await serviceRequestRepository.GetByIdAsync(id);
        return service != null ? await MapToResponseDTO(service) : null;
    }

    public async Task<ServiceResponseDTO?> GetByTicketIdAsync(int ticketId, CancellationToken cancellationToken)
    {
        var service = await serviceRequestRepository.GetByTicketIdAsync(ticketId);
        return service != null ? await MapToResponseDTO(service) : null;
    }

    public async Task<bool> OfferSlotsAsync(ServiceSlotsDTO slots, string techId, CancellationToken cancellationToken)
    {
        var service = await serviceRequestRepository.GetByIdAsync(slots.ServiceId);
        if (service == null)
            throw new ServiceNotFoundException();

        if (service.Status != (short)eDefServiceStatus.Pending)
            throw new ServiceException("Não é possível oferecer horários para este serviço.");
        
        await slotsRepository.ClearSlotsAsync(slots.ServiceId);
        var availableSlots = slots.Slots.Select(slot => new ServiceAvailableSlots
        {
            ServiceRequestId = slots.ServiceId,
            AvailableDate = slot.Date,
            Period = slot.Period,
            CreatedAt = DateTime.Now
        }).ToList();

        await slotsRepository.AddSlotsAsync(availableSlots);

        // Atualizar status do serviço
        service.Status = (short)eDefServiceStatus.OptionsAvailable;
        service.AssignedTechId = techId;
        service.UpdatedAt = DateTime.Now;
        await serviceRequestRepository.UpdateAsync(service);

        return true;
    }

    public async Task<bool> SelectSlotAsync(SelectSlotDTO selection, string userId, CancellationToken cancellationToken)
    {
        var service = await serviceRequestRepository.GetByIdAsync(selection.ServiceId);
        if (service == null)
            throw new ServiceNotFoundException();

        if (service.RequestedById != userId)
            throw new ServiceException("Usuário não autorizado para esta operação.");

        if (service.Status != (short)eDefServiceStatus.OptionsAvailable)
            throw new ServiceException("Não é possível selecionar horário para este serviço.");
        await slotsRepository.SelectSlotAsync(selection.SlotId);
        
        var selectedSlot = await slotsRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Id == selection.SlotId, cancellationToken);

        if (selectedSlot == null)
            throw new ServiceException("Slot não encontrado.");
        service.Status = (short)eDefServiceStatus.Scheduled;
        service.ServiceDate = selectedSlot.AvailableDate;
        service.ServicePeriod = selectedSlot.Period;
        service.UpdatedAt = DateTime.Now;
        await serviceRequestRepository.UpdateAsync(service);

        return true;
    }

    public async Task<bool> StartServiceAsync(int serviceId, string techId, CancellationToken cancellationToken)
    {
        var service = await serviceRequestRepository.GetByIdAsync(serviceId);
        if (service == null)
            throw new ServiceNotFoundException();

        if (service.AssignedTechId != techId)
            throw new ServiceException("Técnico não autorizado para esta operação.");

        if (service.Status != (short)eDefServiceStatus.Scheduled)
            throw new ServiceException("Não é possível iniciar este serviço.");

        service.Status = (short)eDefServiceStatus.InProgress;
        service.UpdatedAt = DateTime.Now;
        await serviceRequestRepository.UpdateAsync(service);

        return true;
    }

    public async Task<bool> CompleteServiceAsync(ServiceReportDTO report, string techId, CancellationToken cancellationToken)
    {
        var service = await serviceRequestRepository.GetByIdAsync(report.ServiceId);
        if (service == null)
            throw new ServiceNotFoundException();

        if (service.AssignedTechId != techId)
            throw new ServiceException("Técnico não autorizado para esta operação.");

        if (service.Status != (short)eDefServiceStatus.InProgress)
            throw new ServiceException("Não é possível finalizar este serviço.");

        service.Status = (short)eDefServiceStatus.Completed;
        service.ServiceReport = report.ServiceReport;
        service.CompletedAt = DateTime.Now;
        service.UpdatedAt = DateTime.Now;
        await serviceRequestRepository.UpdateAsync(service);
        await ticketsRepository.UpdateStatusAsync(service.TicketId, eDefTicketStatus.Resolved, cancellationToken);

        return true;
    }

    public async Task<bool> CancelServiceAsync(int serviceId, string userId, CancellationToken cancellationToken)
    {
        var service = await serviceRequestRepository.GetByIdAsync(serviceId);
        if (service == null)
            throw new ServiceNotFoundException();

        if (service.RequestedById != userId && service.AssignedTechId != userId)
            throw new ServiceException("Usuário não autorizado para esta operação.");

        if (service.Status == (short)eDefServiceStatus.Completed)
            throw new ServiceException("Não é possível cancelar um serviço já concluído.");

        service.Status = (short)eDefServiceStatus.Cancelled;
        service.UpdatedAt = DateTime.Now;
        await serviceRequestRepository.UpdateAsync(service);

        return true;
    }

    public async Task<IEnumerable<ServiceResponseDTO>> GetByTechnicianAsync(string techId, CancellationToken cancellationToken)
    {
        var services = await serviceRequestRepository.GetByTechnicianAsync(techId);
        var result = new List<ServiceResponseDTO>();

        foreach (var service in services)
        {
            result.Add(await MapToResponseDTO(service));
        }

        return result;
    }

    public async Task<IEnumerable<ServiceResponseDTO>> GetByUserAsync(string userId, CancellationToken cancellationToken)
    {
        var services = await serviceRequestRepository.GetByUserAsync(userId);
        var result = new List<ServiceResponseDTO>();

        foreach (var service in services)
        {
            result.Add(await MapToResponseDTO(service));
        }

        return result;
    }

    public async Task<PaginatedResponse<ServiceResponseDTO>> GetAllAsync(PaginatedQuery query, CancellationToken cancellationToken)
    {
        var paginatedServices = await serviceRequestRepository.GetAllAsync(query);
        var result = new List<ServiceResponseDTO>();

        foreach (var service in paginatedServices.Items)
        {
            result.Add(await MapToResponseDTO(service));
        }

        return new PaginatedResponse<ServiceResponseDTO>
        {
            Items = result,
            Count = paginatedServices.Count,
            Page = paginatedServices.Page,
            PageSize = paginatedServices.PageSize
        };
    }

    private async Task<ServiceResponseDTO> MapToResponseDTO(ServiceRequest service)
    {
        var slots = await slotsRepository.GetByServiceRequestIdAsync(service.Id);

        return new ServiceResponseDTO
        {
            Id = service.Id,
            TicketId = service.TicketId,
            RequestedByName = service.RequestedBy?.FullName ?? "N/A",
            AssignedTechName = service.AssignedTech?.FullName,
            ServiceDate = service.ServiceDate,
            ServicePeriodName = GetPeriodName(service.ServicePeriod),
            Status = service.Status,
            StatusName = GetStatusName(service.Status),
            ServiceReport = service.ServiceReport,
            CreatedAt = service.CreatedAt,
            CompletedAt = service.CompletedAt,
            Address = new AddressDTO
            {
                Street = service.Address.Street,
                City = service.Address.City,
                State = service.Address.State,
                ZipCode = service.Address.ZipCode,
                Country = service.Address.Country,
                Complement = service.Address.Complement,
                Reference = service.Address.Reference
            },
            AvailableSlots = slots.Select(slot => new AvailableSlotResponseDTO
            {
                Id = slot.Id,
                AvailableDate = slot.AvailableDate,
                Period = slot.Period,
                PeriodName = GetPeriodName(slot.Period),
                IsSelected = slot.IsSelected
            }).ToList()
        };
    }

    private static string GetPeriodName(short period) => period switch
    {
        (short)eDefServicePeriod.Morning => "Manhã",
        (short)eDefServicePeriod.Afternoon => "Tarde",
        _ => "N/A"
    };

    private static string GetStatusName(short status) => status switch
    {
        (short)eDefServiceStatus.Pending => "Aguardando",
        (short)eDefServiceStatus.OptionsAvailable => "Opções Disponíveis",
        (short)eDefServiceStatus.Scheduled => "Agendado",
        (short)eDefServiceStatus.InProgress => "Em Andamento",
        (short)eDefServiceStatus.Completed => "Concluído",
        (short)eDefServiceStatus.Cancelled => "Cancelado",
        _ => "N/A"
    };
}
