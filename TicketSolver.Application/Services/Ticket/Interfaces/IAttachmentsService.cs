using TicketSolver.Application.Models.Auth;
using TicketSolver.Application.Models.Storage;

namespace TicketSolver.Application.Services.Ticket.Interfaces;

public interface IAttachmentsService
{
    /// <summary>
    /// Anexo de arquivo à ticket
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="ticketId"></param>
    /// <param name="user"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="TicketNotFoundException"></exception>
    Task<FileUploadResult> UploadFileToTicketAsync(CancellationToken cancellationToken, int ticketId, AuthenticatedUser user, FileUploadRequest request);
    
    /// <summary>
    /// Remoção de anexo de ticket
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="attachmentId"></param>
    /// <returns></returns>
    /// <exception cref="AttachmentNotFoundException"></exception>
    Task<bool> DeleteAttachmentAsync(CancellationToken cancellationToken, int attachmentId, AuthenticatedUser user);
    
    /// <summary>
    /// Busca anexos do ticket
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="ticketId"></param>
    /// <returns></returns>
    /// <exception cref="TicketNotFoundException"></exception>
    Task<List<AttachmentDto>> GetTicketAttachmentsAsync(CancellationToken cancellationToken, int ticketId);
}