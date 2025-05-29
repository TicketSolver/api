namespace TicketSolver.Application.Models.Storage;

public class FileUploadRequest
{
    public Stream Stream { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public List<string> Path { get; set; } = [];
    public string ContentType { get; set; } = null!;
}