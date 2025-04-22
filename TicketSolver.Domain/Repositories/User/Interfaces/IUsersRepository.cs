using TicketSolver.Domain.Persistence.Tables.User;
using TicketSolver.Domain.Repositories.Interfaces;

namespace TicketSolver.Domain.Repositories.User.Interfaces;

public interface IUsersRepository : IEFRepositoryBase<Users>
{
    IQueryable<Users> GetByEmail(string email);
    IQueryable<Users> GetByIdentityUserId(string identityUserId);

    IQueryable<Users> GetById(int id);
}