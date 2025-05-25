using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Persistence.Tables.User;
using TicketSolver.Domain.Repositories.User;
using TicketSolver.Infra.EntityFramework.Persistence;

namespace TicketSolver.Infra.EntityFramework.Repositories.User;

public class UsersRepository(EfContext context) : EFRepositoryBase<Users>(context), IUsersRepository
{
    public override IQueryable<Users> GetAll()
    {
        return base.GetAll().AsNoTracking();
    }

    public async Task<IEnumerable<Users>> ExecuteQueryAsync(IQueryable<Users> query,
        CancellationToken cancellationToken = default)
    {
        return await query.ToListAsync(cancellationToken);
    }

    public IQueryable<Users> GetByEmail(string email)
    {
        return GetAll()
            .Where(u => u.Email == email);
    }

    public async Task<Users?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await GetAll()
            .Where(u => u.Email == email).FirstOrDefaultAsync(cancellationToken);
    }


    public IQueryable<Users> GetById(string id)
    {
        return GetAll()
            .Where(u => u.Id == id);
    }

    public async Task<Users?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await GetAll()
            .Where(u => u.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Users> CreateUserAsync(Users user, CancellationToken cancellationToken = default)
    {
        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return user;
    }

    public async Task<Users> UpdateUserAsync(Users user, CancellationToken cancellationToken = default)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);
        
        return user;
    }
}