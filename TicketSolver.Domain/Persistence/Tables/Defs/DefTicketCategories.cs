using TicketSolver.Domain.Enums;

namespace TicketSolver.Domain.Persistence.Tables.Defs;

public class DefTicketCategories : BaseDef<eDefTicketCategories>
{
    public DefTicketCategories()
    {
    }

    public DefTicketCategories(eDefTicketCategories enumValue) : base(enumValue)
    {
    }
}