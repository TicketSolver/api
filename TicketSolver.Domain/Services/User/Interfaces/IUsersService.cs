using TicketSolver.Domain.Persistence.Tables.User;

namespace TicketSolver.Domain.Services.User.Interfaces;

public interface IUsersService
{
    Task<IQueryable<Users>> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<IQueryable<Users>> GetByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken);
    Task<IQueryable<Users>> GetByIdAsync(int id, CancellationToken cancellationToken);
}