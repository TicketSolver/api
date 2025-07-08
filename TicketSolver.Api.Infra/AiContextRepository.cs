using System.Collections.Concurrent;
using TicketSolver.Framework.Domain;

namespace TicketSolver.Api.Infra;

public class AiContextRepository : IAiContextRepository
{
    private readonly ConcurrentDictionary<string, AiContext> _storage = new();

    public Task<AiContext?> GetContext(CancellationToken cancellationToken, Tenant tenant)
    {
        _storage.TryGetValue(tenant.Id, out var ctx);
        return Task.FromResult(ctx);
    }

    public Task AddAiContext(CancellationToken cancellationToken, AiContext context, Tenant tenant)
    {
        _storage[tenant.Id] = context;
        return Task.CompletedTask;
    }
}
