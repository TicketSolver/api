using System.ComponentModel.DataAnnotations;

namespace TicketSolver.Domain.Persistence.Tables.Tenant;

public class Tenants
{
    [Key] public int Id { get; set; }

    public string AdminKey { get; set; }
    public string PublicKey { get; set; }
}