using TicketSolver.Domain.Persistence.Tables.User;
using TicketSolver.Domain.Repositories.User.Interfaces;
using TicketSolver.Domain.Services.User.Interfaces;

namespace TicketSolver.Domain.Services.User;

public class UsersService(
    IUsersRepository usersRepository
) : IUsersService
{
    public Task<IQueryable<Users>> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return Task.FromResult(usersRepository.GetByEmail(email));
    }

    public Task<IQueryable<Users>> GetByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken)
    {   
        return Task.FromResult(usersRepository.GetByIdentityUserId(identityUserId));
    }

    public Task<IQueryable<Users>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return Task.FromResult(usersRepository.GetById(id));
    }
}