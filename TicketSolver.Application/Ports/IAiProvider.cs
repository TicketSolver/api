namespace TicketSolver.Application.Ports;

public interface IAiProvider
{
    Task<string> GenerateTextAsync(string prompt, CancellationToken ct);
    Task<byte[]>  GenerateBinaryAsync(object payload, CancellationToken ct);
}