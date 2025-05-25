using TicketSolver.Domain.Enums;

namespace TicketSolver.Domain.Persistence.Tables.Defs;

public class DefUserSatisfaction : BaseDef<eDefUserSatisfaction>
{
    public DefUserSatisfaction()
    {
    }

    public DefUserSatisfaction(eDefUserSatisfaction value) : base(value)
    {
    }
}