namespace TicketSolver.Domain.Repositories;

public interface IRepositoryBase<TEntity>
{
    IQueryable<TEntity> GetAll();

    Task<IEnumerable<TEntity>> ExecuteQueryAsync(IQueryable<TEntity> query,
        CancellationToken cancellationToken = default);
}