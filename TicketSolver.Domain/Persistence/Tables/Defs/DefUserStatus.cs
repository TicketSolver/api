using TicketSolver.Domain.Enums;

namespace TicketSolver.Domain.Persistence.Tables.Defs;

public class DefUserStatus : BaseDef<eDefUserStatus>
{
    public DefUserStatus()
    {
    }

    public DefUserStatus(eDefUserStatus value) : base(value)
    {
    }
}