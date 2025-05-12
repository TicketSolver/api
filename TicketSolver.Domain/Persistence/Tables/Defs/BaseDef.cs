using System.ComponentModel.DataAnnotations;

namespace TicketSolver.Domain.Persistence.Tables.Defs;

public class BaseDef<TEnum> where TEnum : Enum
{
    
    [Key] public short Id { get; set; }
    public string Name { get; set; }

    public BaseDef()
    {
    }

    public BaseDef(TEnum enumValue)
    {
        Id = Convert.ToInt16(enumValue);
        Name = Enum.GetName(typeof(TEnum), enumValue)
               ?? throw new ArgumentException("Invalid enum value for BaseDef");
    }
}