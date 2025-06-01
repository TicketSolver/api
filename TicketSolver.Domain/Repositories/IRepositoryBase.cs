namespace TicketSolver.Domain.Repositories;

public interface IRepositoryBase<TEntity>
{
    IQueryable<TEntity> GetAll();
    IQueryable<TEntity> GetById<TId>(TId id);
    Task<bool> InsertAsync(CancellationToken cancellationToken, TEntity id);
    Task<bool> DeleteAsync(CancellationToken cancellationToken, TEntity id);
    Task<bool> UpdateAsync(CancellationToken cancellationToken, TEntity id);
    Task<bool> AddRangeAsync(CancellationToken cancellationToken, List<TEntity> entity);
    Task<bool> RemoveRangeAsync(CancellationToken cancellationToken, List<TEntity> entity);

    Task<IEnumerable<TEntity>> ExecuteQueryAsync(IQueryable<TEntity> query,
        CancellationToken cancellationToken = default);
}