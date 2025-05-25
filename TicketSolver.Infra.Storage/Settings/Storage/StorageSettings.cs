namespace TicketSolver.Infra.Storage.Settings.Storage;

public class StorageSettings
{
    public string Provider { get; set; } = null!; // "Aws" ou "Azure"
    public AwsStorageSettings AWS { get; set; } = new();
    public AzureStorageSettings Azure { get; set; } = new();
}