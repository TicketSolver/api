using TicketSolver.Application.Models.Storage;

namespace TicketSolver.Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<FileUploadResult> UploadAsync(FileUploadRequest fileUploadRequest, CancellationToken cancellationToken);
    Task<List<FileUploadResult>> UploadManyAsync(IEnumerable<FileUploadRequest> files, CancellationToken cancellationToken);
    Task DeleteAsync(string fileKey, CancellationToken cancellationToken);
    Task DeleteManyAsync(IEnumerable<string> fileKeys, CancellationToken cancellationToken);
    Task<Stream> DownloadAsync(string fileKey, CancellationToken cancellationToken);
}
