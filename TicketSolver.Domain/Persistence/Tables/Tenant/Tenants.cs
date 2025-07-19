// Domain/Persistence/Tables/Tenant/Tenants.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TicketSolver.Domain.Persistence.Tables.User;

namespace TicketSolver.Domain.Persistence.Tables.Tenant
{
    public class Tenants : EntityDates
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        
        [Column(TypeName = "uuid")]
        public Guid AdminKey { get; set; }
        
        [Column(TypeName = "uuid")]
        public Guid PublicKey { get; set; }

        public bool IsConfigured { get; set; }

        [Required]
        public ApplicationType ApplicationType { get; set; }

        // inicialização correta da coleção
        public ICollection<Users> Users { get; set; } = new List<Users>();
    }
}