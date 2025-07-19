using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Ticket;
using TicketSolver.Infra.EntityFramework.Persistence;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts.Interfaces;

namespace TicketSolver.Infra.EntityFramework.Repositories.Ticket;

public class AttachmentsRepository(IEfContext context) : EFRepositoryBase<Attachments>(context), IAttachmentsRepository
{
    
}