using TicketSolver.Domain.Enums;

namespace TicketSolver.Domain.Persistence.Tables.Defs;

public class DefTicketPriorities : BaseDef<eDefTicketPriorities>
{
    public DefTicketPriorities()
    {
    }

    public DefTicketPriorities(eDefTicketPriorities value) : base(value)
    {
    }
}