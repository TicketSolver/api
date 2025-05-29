using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Ticket;
using TicketSolver.Infra.EntityFramework.Persistence;

namespace TicketSolver.Infra.EntityFramework.Repositories.Ticket;

public class TicketUsersRepository(EfContext context) : EFRepositoryBase<TicketUsers>(context), ITicketUsersRepository
{
    public async Task<bool> IsUserAssignedToTicketAsync(CancellationToken cancellationToken, string userId, int ticketId)
    {
        return await GetAll()
            .AsNoTracking()
            .Where(tu => tu.TicketId == ticketId && tu.UserId == userId)
            .AnyAsync(cancellationToken);
    }

    public IQueryable<TicketUsers> GetByUserId(string userId)
    {
        return GetAll()
            .Where(tu => tu.UserId == userId);
    }
}