using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TicketSolver.Application.Interfaces.Services;
using TicketSolver.Application.Models.Storage;
using TicketSolver.Domain.Enums;
using TicketSolver.Infra.Storage.Settings.Storage;

namespace TicketSolver.Infra.Storage.Providers.AWS;

public class S3FileStorageService : FileStorageServiceBase, IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly AwsStorageSettings _settings;
    private readonly TransferUtility _transferUtility;

    public S3FileStorageService(IAmazonS3 s3Client, IOptions<StorageSettings> options)
    {
        _s3Client = s3Client;
        _settings = options.Value.AWS;
        _transferUtility = new TransferUtility(_s3Client);
    }

    public async Task<FileUploadResult> UploadAsync(FileUploadRequest fileUploadRequest,
        CancellationToken cancellationToken)
    {
        var request = new TransferUtilityUploadRequest
        {
            InputStream = fileUploadRequest.Stream,
            Key = BuildPath(fileUploadRequest.FileName, fileUploadRequest.Path),
            BucketName = _settings.BucketName,
            ContentType = fileUploadRequest.ContentType,
        };

        await _transferUtility.UploadAsync(request, cancellationToken: cancellationToken);

        var url = $"https://{_settings.BucketName}.s3.{_settings.Region}.amazonaws.com/{fileUploadRequest.FileName}";

        return new FileUploadResult
        {
            Key = request.Key,
            Url = url,
            Provider = eDefStorageProviders.Aws
        };
    }

    public async Task<List<FileUploadResult>> UploadManyAsync(
        IEnumerable<FileUploadRequest> files,
        CancellationToken cancellationToken)
    {
        var tasks = files.Select(f => UploadAsync(f, cancellationToken));
        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }


    public async Task DeleteAsync(string fileKey, CancellationToken cancellationToken)
    {
        await _s3Client.DeleteObjectAsync(_settings.BucketName, fileKey, cancellationToken: cancellationToken);
    }

    public async Task DeleteManyAsync(IEnumerable<string> fileKeys, CancellationToken cancellationToken)
    {
        var deleteRequest = new Amazon.S3.Model.DeleteObjectsRequest
        {
            BucketName = _settings.BucketName,
            Objects = fileKeys.Select(k => new Amazon.S3.Model.KeyVersion { Key = k }).ToList()
        };

        await _s3Client.DeleteObjectsAsync(deleteRequest, cancellationToken: cancellationToken);
    }

    public async Task<Stream> DownloadAsync(string fileKey, CancellationToken cancellationToken)
    {
        var response = await _s3Client.GetObjectAsync(_settings.BucketName, fileKey, cancellationToken: cancellationToken);
        return response.ResponseStream;
    }
}