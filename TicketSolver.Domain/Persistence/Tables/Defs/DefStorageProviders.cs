using TicketSolver.Domain.Enums;

namespace TicketSolver.Domain.Persistence.Tables.Defs;

public class DefStorageProviders : BaseDef<eDefStorageProviders>
{
    public DefStorageProviders()
    {
    }

    public DefStorageProviders(eDefStorageProviders enumValue) : base(enumValue)
    {
    }
}