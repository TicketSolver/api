using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TicketSolver.Domain.Persistence.Tables.User;

namespace TicketSolver.Domain.Persistence.Tables.Tenant;

public class Tenants : EntityDates
{
    [Key] public int Id { get; set; }

    public string Name { get; set; }
    
    [Column(TypeName = "uuid")]
    public Guid AdminKey { get; set; }
    
    [Column(TypeName = "uuid")]
    public Guid PublicKey { get; set; }
    public bool IsConfigured { get; set; }

    public ICollection<Users> Users { get; set; } = [];
}