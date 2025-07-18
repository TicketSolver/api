using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Framework.Domain;

namespace TicketSolver.Api.Infra
{
    public class AiContextRepository : IAiContextRepository
    {
        
        private readonly ConcurrentDictionary<int, AiContext> _storage = new();

        public Task<AiContext?> GetContext(CancellationToken cancellationToken, Tenants tenant)
        {
            _storage.TryGetValue(tenant.Id, out var ctx);
            return Task.FromResult(ctx);
        }

        public Task AddAiContext(CancellationToken cancellationToken, AiContext context, Tenants tenant)
        {
            _storage[tenant.Id] = context;
            return Task.CompletedTask;
        }
    }
}