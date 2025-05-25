using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
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
    
    [NotMapped]
    public List<ChatMessageDto> Messages
    {
        get => string.IsNullOrEmpty(ChatHistory) 
            ? new List<ChatMessageDto>() 
            : JsonSerializer.Deserialize<List<ChatMessageDto>>(ChatHistory) ?? new List<ChatMessageDto>();
            
        set => ChatHistory = JsonSerializer.Serialize(value ?? new List<ChatMessageDto>());
    }
}

public class ChatMessageDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int SenderId { get; set; }
    public string SenderType { get; set; } = "User"; // User, Technician, AI
    public string SenderName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string MessageType { get; set; } = "Text"; // Text, File, Image
    public string? AttachmentUrl { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}