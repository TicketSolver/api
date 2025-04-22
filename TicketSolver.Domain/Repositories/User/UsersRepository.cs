using TicketSolver.Domain.Persistence;
using TicketSolver.Domain.Persistence.Tables.User;
using TicketSolver.Domain.Repositories.User.Interfaces;

namespace TicketSolver.Domain.Repositories.User;

public class UsersRepository(EFContext context) : EFRepositoryBase<Users>(context), IUsersRepository
{
    public IQueryable<Users> GetByEmail(string email)
    {
        return GetAll()
            .Where(u=> u.Email == email);
    }
    
    public IQueryable<Users> GetByIdentityUserId(string identityUserId)
    {
        return GetAll()
            .Where(u=> u.IdentityUserId == identityUserId);
    }
    
    public IQueryable<Users> GetById(int id)
    {
        return GetAll()
            .Where(u=> u.Id == id);
    }
}