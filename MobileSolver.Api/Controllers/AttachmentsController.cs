using Microsoft.AspNetCore.Mvc;
using MobileSolver.Api.Models;
using TicketSolver.Application.Exceptions.Http;
using TicketSolver.Application.Exceptions.Ticket;
using TicketSolver.Application.Models.Storage;
using TicketSolver.Application.Services.Ticket.Interfaces;

namespace MobileSolver.Api.Controllers;

public class AttachmentsController(
    IAttachmentsService attachmentsService
) : ShellController
{
    [HttpPost("ticket/{ticketId}/upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadFilesToTicket([FromRoute] int ticketId, IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null)
            return BadRequest("Nenhum arquivo enviado.");

        try
        {
            await using var stream = file.OpenReadStream();
            var request = new FileUploadRequest
            {
                FileName = file.FileName,
                Stream = stream,
                ContentType = file.ContentType
            };

            var uploadedFile =
                await attachmentsService.UploadFileToTicketAsync(cancellationToken, ticketId, AuthenticatedUser,
                    request);
            return Ok(ApiResponse.Ok(uploadedFile));
        }
        catch (TicketNotFoundException)
        {
            throw new NotFoundException("Ticket não encontrado!");
        }
    }

    [HttpDelete("{attachmentId}")]
    public async Task<IActionResult> DeleteFile([FromRoute] int attachmentId, CancellationToken cancellationToken)
    {
        try
        {
            var success =
                await attachmentsService.DeleteAttachmentAsync(cancellationToken, attachmentId, AuthenticatedUser);

            if (!success)
                return NotFound($"Não foi possível remover o anexo [{attachmentId}]'.");

            return Ok(ApiResponse.Ok(new { }));
        }
        catch (AttachmentNotFoundException)
        {
            throw new NotFoundException("Anexo não encontrado!");
        }
    }

    [HttpGet("{ticketId}")]
    public async Task<IActionResult> GetTicketAttachmentsAsync(CancellationToken cancellationToken, int ticketId)
    {
        try
        {
            var attachments = await attachmentsService.GetTicketAttachmentsAsync(cancellationToken, ticketId);
            return Ok(ApiResponse.Ok(attachments));
        }
        catch (TicketNotFoundException)
        {
            throw new NotFoundException("Ticket não encontrado!");
        }
    }
}