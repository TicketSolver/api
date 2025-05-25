using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using TicketSolver.Domain.Models.Chat;
using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace TicketSolver.Domain.Persistence.Tables.Chat;

[Table("Chat")]
public class TicketChat
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [ForeignKey("Ticket")]
    public int TicketId { get; set; }

    [Required]
    [Column(TypeName = "jsonb")]
    public string ChatHistory { get; set; } = "[]";

    public int TotalMessages { get; set; } = 0;

    public DateTime? LastMessageAt { get; set; }

    [Required]
    [Column(TypeName = "timestamp")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column(TypeName = "timestamp")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual Tickets Ticket { get; set; } = null!;
    public bool IsArchived { get; set; } = false;
    [NotMapped]
    public List<Message> Messages
    {
        get => string.IsNullOrEmpty(ChatHistory) 
            ? new List<Message>() 
            : JsonSerializer.Deserialize<List<Message>>(ChatHistory) ?? new List<Message>();
            
        set => ChatHistory = JsonSerializer.Serialize(value ?? new List<Message>());
    }
}

