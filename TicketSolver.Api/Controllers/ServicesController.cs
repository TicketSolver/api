using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSolver.Api.Models;
using TicketSolver.Application.Exceptions.Service;
using TicketSolver.Application.Exceptions.Ticket;
using TicketSolver.Application.Models.Service;
using TicketSolver.Application.Services.Service.Interfaces;
using TicketSolver.Domain.Models;

namespace TicketSolver.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController(IServiceRequestService service) : ShellController
{
    [HttpPost("request")]
[Authorize(Roles = "1,2,3")]
public async Task<ActionResult<ServiceResponseDTO>> RequestService(
    [FromBody] ServiceRequestInputDTO inputRequest, 
    CancellationToken cancellationToken)
{
    try
    {
        if (!AuthenticatedUser.IsAuthenticated || string.IsNullOrEmpty(AuthenticatedUser.UserId))
        {
            return Unauthorized(ApiResponse.Fail("Usuário não autenticado. Faça login novamente."));
        }
        var request = new ServiceRequestDTO
        {
            TicketId = inputRequest.TicketId,
            RequestedById = AuthenticatedUser.UserId,
            Address = inputRequest.Address,
            Comments = inputRequest.Comments
        };
        
        var result = await service.RequestServiceAsync(request, cancellationToken);
        return Ok(ApiResponse.Ok(result, "Atendimento solicitado com sucesso!"));
    }
    catch (TicketNotFoundException)
    {
        return NotFound(ApiResponse.Fail("Ticket não encontrado."));
    }
    catch (ServiceException ex)
    {
        return BadRequest(ApiResponse.Fail(ex.Message));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro geral: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return BadRequest(ApiResponse.Fail($"Erro interno: {ex.Message}"));
    }
}



    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ServiceResponseDTO>> GetService(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetByIdAsync(id, cancellationToken);
            if (result == null)
                return NotFound(ApiResponse.Fail("Atendimento não encontrado."));

            return Ok(ApiResponse.Ok(result));
        }
        catch (ServiceException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("ticket/{ticketId}")]
    [Authorize]
    public async Task<ActionResult<ServiceResponseDTO>> GetServiceByTicket(int ticketId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetByTicketIdAsync(ticketId, cancellationToken);
            if (result == null)
                return NotFound(ApiResponse.Fail("Atendimento não encontrado para este ticket."));

            return Ok(ApiResponse.Ok(result));
        }
        catch (ServiceException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPost("{serviceId}/offer-slots")]
    [Authorize(Roles = "1,2")]
    public async Task<ActionResult> OfferSlots(
        int serviceId,
        [FromBody] List<AvailableSlotDTO> slots, 
        CancellationToken cancellationToken)
    {
        try
        {
            var slotsDto = new ServiceSlotsDTO { ServiceId = serviceId, Slots = slots };
            await service.OfferSlotsAsync(slotsDto, AuthenticatedUser.UserId, cancellationToken);
            return Ok(ApiResponse.Ok("", "Horários oferecidos com sucesso!"));
        }
        catch (ServiceNotFoundException)
        {
            return NotFound(ApiResponse.Fail("Atendimento não encontrado."));
        }
        catch (ServiceException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPost("select-slot")]
    [Authorize(Roles = "1,3")]
    public async Task<ActionResult> SelectSlot([FromBody] SelectSlotDTO selection, CancellationToken cancellationToken)
    {
        try
        {
            await service.SelectSlotAsync(selection, AuthenticatedUser.UserId, cancellationToken);
            return Ok(ApiResponse.Ok("", "Horário selecionado com sucesso!"));
        }
        catch (ServiceNotFoundException)
        {
            return NotFound(ApiResponse.Fail("Atendimento não encontrado."));
        }
        catch (ServiceException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPatch("{serviceId}/start")]
    [Authorize(Roles = "1,2")]
    public async Task<ActionResult> StartService(int serviceId, CancellationToken cancellationToken)
    {
        try
        {
            await service.StartServiceAsync(serviceId, AuthenticatedUser.UserId, cancellationToken);
            return Ok(ApiResponse.Ok("", "Atendimento iniciado com sucesso!"));
        }
        catch (ServiceNotFoundException)
        {
            return NotFound(ApiResponse.Fail("Atendimento não encontrado."));
        }
        catch (ServiceException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPost("{serviceId}/complete")]
    [Authorize(Roles = "1,2")]
    public async Task<ActionResult> CompleteService(
        int serviceId,
        [FromBody] string serviceReport, 
        CancellationToken cancellationToken)
    {
        try
        {
            var reportDto = new ServiceReportDTO { ServiceId = serviceId, ServiceReport = serviceReport };
            await service.CompleteServiceAsync(reportDto, AuthenticatedUser.UserId, cancellationToken);
            return Ok(ApiResponse.Ok("", "Atendimento finalizado com sucesso!"));
        }
        catch (ServiceNotFoundException)
        {
            return NotFound(ApiResponse.Fail("Atendimento não encontrado."));
        }
        catch (ServiceException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPatch("{serviceId}/cancel")]
    [Authorize]
    public async Task<ActionResult> CancelService(int serviceId, CancellationToken cancellationToken)
    {
        try
        {
            await service.CancelServiceAsync(serviceId, AuthenticatedUser.UserId, cancellationToken);
            return Ok(ApiResponse.Ok("", "Atendimento cancelado com sucesso!"));
        }
        catch (ServiceNotFoundException)
        {
            return NotFound(ApiResponse.Fail("Atendimento não encontrado."));
        }
        catch (ServiceException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("technician/my-services")]
    [Authorize(Roles = "1,2")]
    public async Task<ActionResult<IEnumerable<ServiceResponseDTO>>> GetMyServices(CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetByTechnicianAsync(AuthenticatedUser.UserId, cancellationToken);
            return Ok(ApiResponse.Ok(result));
        }
        catch (ServiceException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("user/my-services")]
    [Authorize(Roles = "1,3")]
    public async Task<ActionResult<IEnumerable<ServiceResponseDTO>>> GetUserServices(CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetByUserAsync(AuthenticatedUser.UserId, cancellationToken);
            return Ok(ApiResponse.Ok(result));
        }
        catch (ServiceException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet]
    [Authorize(Roles = "1")]
    public async Task<ActionResult<PaginatedResponse<ServiceResponseDTO>>> GetAllServices(
        [FromQuery] PaginatedQuery query, 
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetAllAsync(query, cancellationToken);
            return Ok(ApiResponse.Ok(result));
        }
        catch (ServiceException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}
