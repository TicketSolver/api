using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TicketSolver.Infra.EntityFramework.Persistence.Contexts.Interfaces;

public interface IDbContext
{
    // Acesso genérico a DbSets
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    // Entry e Entry<TEntity>
    EntityEntry Entry(object entity);
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

    // SaveChanges
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // Transações e acesso ao banco
    DatabaseFacade Database { get; }

    // Change tracker
    ChangeTracker ChangeTracker { get; }

    // Informações de modelo
    IModel Model { get; }
}