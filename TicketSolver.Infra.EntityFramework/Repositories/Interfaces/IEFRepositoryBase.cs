namespace TicketSolver.Infra.EntityFramework.Repositories.Interfaces;

public interface IEFRepositoryBase<TEntity> where TEntity : class
{
    IQueryable<TEntity> GetAll();
    string GetTenant();
    IQueryable<TEntity> GetById(int id);
    Task<TEntity> GetById(CancellationToken cancellationToken, int id);

    Task<TEntity> FindAsync(CancellationToken cancellationToken, int id);

    Task<IList<TEntity>> GetListAsync(CancellationToken cancellationToken);

    Task<bool> InsertAsync(CancellationToken cancellationToken, TEntity entity);

    Task<bool> UpdateAsync(CancellationToken cancellationToken, TEntity entity);

    Task<bool> UpdateAsync(CancellationToken cancellationToken, List<TEntity> entityList);

    Task<bool> DeleteAsync(CancellationToken cancellationToken, TEntity entity);

    Task<bool> UpdateRangeAsync(CancellationToken cancellationToken, List<TEntity> entity);

    Task<bool> AddRangeAsync(CancellationToken cancellationToken, List<TEntity> entity);

    Task<bool> RemoveRangeAsync(CancellationToken cancellationToken, List<TEntity> entity);

    Task<int> DirectUpdateAsync(CancellationToken cancellationToken,
        IQueryable<TEntity> entity, Dictionary<string, object> dbparams);

    Task<int> DirectUpdateAsync(CancellationToken cancellationToken,
        int Id, Dictionary<string, object> dbparams);

    Task<int> DirectUpdateAsync(CancellationToken cancellationToken,
        List<int> Ids, Dictionary<string, object> dbparams);
}