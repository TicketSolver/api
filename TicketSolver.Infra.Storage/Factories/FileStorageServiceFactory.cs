using Microsoft.Extensions.DependencyInjection;
using TicketSolver.Application.Interfaces.Factories;
using TicketSolver.Application.Interfaces.Services;
using TicketSolver.Domain.Enums;
using TicketSolver.Infra.Storage.Providers.AWS;
using TicketSolver.Infra.Storage.Providers.Azure;

namespace TicketSolver.Infra.Storage.Factories;

public class FileStorageServiceFactory(IServiceProvider serviceProvider) : IFileStorageServiceFactory
{
    public IFileStorageService Create(eDefStorageProviders provider)
    {
        return provider switch
        {
            eDefStorageProviders.Aws => serviceProvider.GetRequiredService<S3FileStorageService>(),
            eDefStorageProviders.Azure => serviceProvider.GetRequiredService<AzureBlobStorageService>(),
            _ => throw new ArgumentException("Provider inválido")
        };
    }
}
