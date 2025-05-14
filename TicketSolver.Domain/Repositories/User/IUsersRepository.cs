using TicketSolver.Domain.Persistence.Tables.User;

namespace TicketSolver.Domain.Repositories.User;

public interface IUsersRepository : IRepositoryBase<Users>
{
    IQueryable<Users> GetByEmail(string email);
    Task<Users?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    IQueryable<Users> GetById(string id);
    Task<Users?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Users> CreateUserAsync(Users user, CancellationToken cancellationToken);
    
    Task<Users> UpdateUserAsync(Users user, CancellationToken cancellationToken);
}