using TicketSolver.Domain.Enums;

namespace TicketSolver.Application.Models.Storage;

public class FileUploadResult
{
    public string Key { get; set; }
    public string Url { get; set; }
    public eDefStorageProviders Provider { get; set; }
}