using TicketSolver.Domain.Enums;

namespace TicketSolver.Domain.Persistence.Tables.Defs;

public class DefTicketUserRoles : BaseDef<eDefTicketUserRoles>
{
    public DefTicketUserRoles()
    {
    }

    public DefTicketUserRoles(eDefTicketUserRoles value) : base(value)
    {
    }
}