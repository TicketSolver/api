using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Ticket;
using TicketSolver.Infra.EntityFramework.Persistence;

namespace TicketSolver.Infra.EntityFramework.Repositories.Ticket;

public class AttachmentsRepository(EfContext context) : EFRepositoryBase<Attachments>(context), IAttachmentsRepository
{
    
}