using TicketSolver.Domain.Enums;

namespace TicketSolver.Domain.Persistence.Tables.Defs;

public class DefTicketStatus : BaseDef<eDefTicketStatus>
{
    public DefTicketStatus()
    {
    }

    public DefTicketStatus(eDefTicketStatus value) : base(value)
    {
    }
}