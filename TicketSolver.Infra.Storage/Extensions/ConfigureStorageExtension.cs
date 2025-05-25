using Amazon.S3;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketSolver.Application.Contexts;
using TicketSolver.Application.Contexts.Interfaces;
using TicketSolver.Application.Interfaces.Factories;
using TicketSolver.Infra.Storage.Factories;
using TicketSolver.Infra.Storage.Providers.AWS;
using TicketSolver.Infra.Storage.Providers.Azure;

namespace TicketSolver.Infra.Storage.Extensions;

public static class ConfigureStorageExtension
{
    public static void ConfigureStorage(this IServiceCollection serviceCollection, IConfiguration config)
    {
        var storageSection = config.GetSection("Storage");
        var provider = storageSection["Provider"];

        // Azure
        var azureConnStr = storageSection.GetSection("Azure")["ConnectionString"];
        serviceCollection.AddSingleton(new BlobServiceClient(azureConnStr));

        // AWS
        serviceCollection.AddDefaultAWSOptions(config.GetAWSOptions("Storage:AWS"));
        serviceCollection.AddAWSService<IAmazonS3>();
        
        serviceCollection.AddTransient<S3FileStorageService>();
        serviceCollection.AddTransient<AzureBlobStorageService>();
        serviceCollection.AddTransient<IFileStorageServiceFactory, FileStorageServiceFactory>();
        serviceCollection.AddTransient<IFileStorageContext, FileStorageContext>();
    }
}