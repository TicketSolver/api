﻿using Microsoft.EntityFrameworkCore;
using TicketSolver.Application.Contexts.Interfaces;
using TicketSolver.Application.Exceptions.Ticket;
using TicketSolver.Application.Models.Auth;
using TicketSolver.Application.Models.Storage;
using TicketSolver.Application.Services.Ticket.Interfaces;
using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Ticket;

namespace TicketSolver.Application.Services.Ticket;

public class BaseAttachmentsService<TTickets>(
    IAttachmentsRepository attachmentsRepository,
    IFileStorageContext fileStorageContext,
    ITicketsRepository<TTickets> ticketsRepository
) : IAttachmentsService
where TTickets : Tickets
{
    public async Task<FileUploadResult> UploadFileToTicketAsync(CancellationToken cancellationToken, int ticketId,
        AuthenticatedUser user,
        FileUploadRequest request)
    {
        var ticketExists = await ticketsRepository
            .ExistsAsync(ticketId, cancellationToken);

        if (!ticketExists)
            throw new TicketNotFoundException();

        var uploadedFile = await fileStorageContext.UploadAsync(request, cancellationToken);

        var attachment = new Attachments
        {
            TicketId = ticketId,
            UserId = user.UserId,
            FileName = request.FileName,
            Url = uploadedFile.Url,
            FileSize = request.Stream.Length,
            Key = uploadedFile.Key,
            DefStorageProviderId = (short)uploadedFile.Provider
        };

        await attachmentsRepository.InsertAsync(cancellationToken, attachment);

        return uploadedFile;
    }

    public async Task<bool> DeleteAttachmentAsync(CancellationToken cancellationToken, int attachmentId,
        AuthenticatedUser user)
    {
        var attachment = await attachmentsRepository
            .GetById(attachmentId)
            .Where(a => a.UserId == user.UserId)
            .AsNoTracking()
            .Select(a => new { a.Key, a.DefStorageProviderId })
            .FirstOrDefaultAsync(cancellationToken);

        if (attachment is null)
            throw new AttachmentNotFoundException();

        await fileStorageContext.DeleteAsync(
            attachment.Key,
            (eDefStorageProviders)attachment.DefStorageProviderId,
            cancellationToken
        );

        var deletedItem = await attachmentsRepository.GetById(attachmentId)
            .ExecuteDeleteAsync(cancellationToken);

        return deletedItem == 1;
    }
    
    public async Task<List<AttachmentDto>> GetTicketAttachmentsAsync(CancellationToken cancellationToken, int ticketId)
    {
        var ticketExists = await ticketsRepository.GetById(ticketId).AnyAsync(cancellationToken);
        if(!ticketExists)
            throw new TicketNotFoundException();
        
        return await attachmentsRepository.GetAll()
            .AsNoTracking()
            .Where(at => at.TicketId == ticketId)
            .Select(at => new AttachmentDto()
            {
                Id = at.Id,
                Url = at.Url,
                FileName = at.FileName
            })
            .ToListAsync(cancellationToken);
    }
}