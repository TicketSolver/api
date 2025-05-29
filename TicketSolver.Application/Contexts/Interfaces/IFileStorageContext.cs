using TicketSolver.Application.Models.Storage;
using TicketSolver.Domain.Enums;

namespace TicketSolver.Application.Contexts.Interfaces;

public interface IFileStorageContext
{
    Task<FileUploadResult> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(string fileKey, eDefStorageProviders provider, CancellationToken cancellationToken);
}