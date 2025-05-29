using Microsoft.Extensions.Configuration;
using TicketSolver.Application.Contexts.Interfaces;
using TicketSolver.Application.Interfaces.Factories;
using TicketSolver.Application.Models.Storage;
using TicketSolver.Domain.Enums;

namespace TicketSolver.Application.Contexts;

public class FileStorageContext(
    IFileStorageServiceFactory storageServiceFactory,
    IConfiguration configuration
) : IFileStorageContext
{
    public async Task<FileUploadResult> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken)
    {
        var storageService = storageServiceFactory.Create(await GetProvider(request));
        return await storageService.UploadAsync(request, cancellationToken);
    }

    private async Task<eDefStorageProviders> GetProvider(FileUploadRequest request)
    {
        /*
         * Pode ser adicionada lógica na definição do provider como:
         * 1. appSettings.json
         * 2. Env var
         * 3. Consultas no banco
         * 4. Tipo de arquivo
         * 5. Tamanho de arquivo
         * 6. Escolher o provedor com mais espaço
         * 7. etc.
         */

        return GetAppSettingsStorageProvider();
    }

    public async Task DeleteAsync(string fileKey, eDefStorageProviders provider, CancellationToken cancellationToken)
    {
        var storageService = storageServiceFactory.Create(provider);
        await storageService.DeleteAsync(fileKey, cancellationToken);
    }

    private eDefStorageProviders GetAppSettingsStorageProvider()
    {
        var providerConfig = configuration["Storage:Provider"];

        if (string.IsNullOrWhiteSpace(providerConfig))
            throw new ArgumentException("Storage:Provider não configurado no appsettings.");

        if (!Enum.TryParse<eDefStorageProviders>(providerConfig, ignoreCase: true, out var providerEnum) ||
            !Enum.IsDefined(typeof(eDefStorageProviders), providerEnum))
        {
            throw new ArgumentException($"Storage provider inválido: {providerConfig}");
        }

        return providerEnum;
    }
}