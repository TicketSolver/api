using System.Diagnostics;
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
        await Context.Users.AddAsync(user, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);

        return user;
    }

    public async Task<Users> UpdateUserAsync(Users user, CancellationToken cancellationToken = default)
    {
        Context.Users.Update(user);
        await Context.SaveChangesAsync(cancellationToken);
        
        return user;
    }

    public Task<IQueryable<Users>> GetByTenantAsyc(int tenantId, CancellationToken cancellationToken = default)
    {
        var a = Task.FromResult(GetAll()
            .Where(u => u.TenantId == tenantId));
        Console.Out.WriteLine($" COUNT: {a.Result.Count()}");
        return a;
    }
    
    public override async Task<bool> DeleteAsync(CancellationToken cancellationToken, Users user)
    {
        Context.Users.Remove(user);
        var result = await Context.SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}