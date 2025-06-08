using Microsoft.EntityFrameworkCore;
using TicketSolver.Application.Exceptions.Ticket;
using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Ticket;
using TicketSolver.Infra.EntityFramework.Persistence;

namespace TicketSolver.Infra.EntityFramework.Repositories.Ticket;

public class TicketUsersRepository(EfContext context) : EFRepositoryBase<TicketUsers>(context), ITicketUsersRepository
{
    public async Task<bool> IsUserAssignedToTicketAsync(CancellationToken cancellationToken, string userId,
        int ticketId)
    {
        return await DbSet
            .AnyAsync(tu => tu.UserId == userId && tu.TicketId == ticketId, cancellationToken);
    }


    public IQueryable<TicketUsers> GetByUserId(string userId)
    {
        return GetAll()
            .Where(tu => tu.UserId == userId);
    }

    public async Task UnassignUserToTicketAsync(CancellationToken cancellationToken, string userId, int ticketId)
    {
        var ticketUser = await GetByUserId(userId)
            .Where(tu => tu.TicketId == ticketId)
            .ExecuteDeleteAsync(cancellationToken);

        if (ticketUser == 0)
            throw new TicketException("Usuário não vinculado ao ticket");
    }

    public async Task<bool> HasTechnicianAssignedAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(tu => tu.TicketId == ticketId &&
                            tu.DefTicketUserRoleId == (short)eDefTicketUserRoles.Responder,
                cancellationToken);
    }

    public async Task AssignTechnicianToTicketAsync(int ticketId, string technicianId,
        CancellationToken cancellationToken = default)
    {
        // Verifica se já existe atribuição
        var existingAssignment = await DbSet
            .FirstOrDefaultAsync(tu => tu.TicketId == ticketId &&
                                       tu.DefTicketUserRoleId == (short)eDefTicketUserRoles.Responder,
                cancellationToken);

        if (existingAssignment != null)
        {
            // Atualiza o técnico atribuído
            existingAssignment.UserId = technicianId;
            existingAssignment.AddedAt = DateTime.Now;
            DbSet.Update(existingAssignment);
        }
        else
        {
            // Cria nova atribuição
            var ticketUser = new TicketUsers
            {
                UserId = technicianId,
                TicketId = ticketId,
                DefTicketUserRoleId = (short)eDefTicketUserRoles.Responder,
                AddedAt = DateTime.Now
            };
            await DbSet.AddAsync(ticketUser, cancellationToken);
        }

        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> GetAvailableTechniciansAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Busca técnicos (role 2) que têm menos tickets atribuídos
            var technicianWorkload = await (
                from user in Context.Users
                where user.DefUserTypeId == 2 // Técnicos
                let assignedTickets = Context.TicketUsers
                    .Where(tu => tu.UserId == user.Id && 
                                 tu.DefTicketUserRoleId == (short)eDefTicketUserRoles.Responder)
                    .Join(Context.Tickets, 
                        tu => tu.TicketId, 
                        t => t.Id, 
                        (tu, t) => t)
                    .Count(t => t.Status != (short)eDefTicketStatus.Closed && 
                                t.Status != (short)eDefTicketStatus.Resolved)
                orderby assignedTickets
                select new { UserId = user.Id, TicketCount = assignedTickets } // ← Removido o int.Parse!
            ).ToListAsync(cancellationToken);

            // Retorna técnicos com menor carga de trabalho (máximo 10 tickets ativos)
            return technicianWorkload
                .Where(tw => tw.TicketCount < 10)
                .Take(5) // Top 5 técnicos disponíveis
                .Select(tw => tw.UserId); // ← Agora retorna string diretamente
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro em GetAvailableTechniciansAsync: {ex.Message}");
            return new List<string>();
        }
    }

    public async Task<string> GetAssignedTechnicianNameAsync(int ticketId,
        CancellationToken cancellationToken = default)
    {
        var technician = await (
            from tu in DbSet
            join user in Context.Users on tu.UserId equals user.Id
            where tu.TicketId == ticketId &&
                  tu.DefTicketUserRoleId == (short)eDefTicketUserRoles.Responder
            select user.FullName
        ).FirstOrDefaultAsync(cancellationToken);

        return technician ?? "Não atribuído";
    }
}