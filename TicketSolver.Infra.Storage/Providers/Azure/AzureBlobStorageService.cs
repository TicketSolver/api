using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using TicketSolver.Application.Interfaces.Services;
using TicketSolver.Application.Models.Storage;
using TicketSolver.Domain.Enums;
using TicketSolver.Infra.Storage.Settings.Storage;

namespace TicketSolver.Infra.Storage.Providers.Azure;

public class AzureBlobStorageService : FileStorageServiceBase, IFileStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly AzureStorageSettings _settings;

    public AzureBlobStorageService(IOptions<StorageSettings> options)
    {
        _settings = options.Value.Azure;
        _containerClient = new BlobContainerClient(_settings.ConnectionString, _settings.ContainerName);
        _containerClient.CreateIfNotExists();
    }

    public async Task<FileUploadResult> UploadAsync(FileUploadRequest fileUploadRequest, CancellationToken cancellationToken)
    {
        var uniqueKey = BuildPath(fileUploadRequest.FileName, fileUploadRequest.Path);
        var blobClient = _containerClient.GetBlobClient(uniqueKey);
        await blobClient.UploadAsync(fileUploadRequest.Stream, new BlobHttpHeaders { ContentType = fileUploadRequest.ContentType }, cancellationToken: cancellationToken);

        return new FileUploadResult
        {
            Key = uniqueKey,
            Url = blobClient.Uri.ToString(),
            Provider = eDefStorageProviders.Azure
        };
    }

    public async Task<List<FileUploadResult>> UploadManyAsync(IEnumerable<FileUploadRequest> files, CancellationToken cancellationToken)
    {
        var tasks = files.Select(f => UploadAsync(f, cancellationToken));
        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }

    public async Task DeleteAsync(string fileKey, CancellationToken cancellationToken)
    {
        var blobClient = _containerClient.GetBlobClient(fileKey);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
    
    public async Task DeleteManyAsync(IEnumerable<string> fileKeys, CancellationToken cancellationToken)
    {
        var tasks = fileKeys.Select(async key =>
        {
            var blobClient = _containerClient.GetBlobClient(key);
            await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        });

        await Task.WhenAll(tasks);
    }

    public async Task<Stream> DownloadAsync(string fileKey, CancellationToken cancellationToken)
    {
        var blobClient = _containerClient.GetBlobClient(fileKey);
        var response = await blobClient.DownloadAsync(cancellationToken);
        return response.Value.Content;
    }
}
