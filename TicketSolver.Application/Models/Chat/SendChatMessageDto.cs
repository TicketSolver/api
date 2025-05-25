using System.ComponentModel.DataAnnotations;

namespace TicketSolver.Application.Models.Chat;

public class SendChatMessageDto
{
    public int TicketId { get; set; }

    [Required(ErrorMessage = "A mensagem é obrigatória")]
    [StringLength(2000, ErrorMessage = "A mensagem deve ter no máximo 2000 caracteres")]
    public string Message { get; set; } = string.Empty;

    [StringLength(50)]
    public string? MessageType { get; set; } = "Text";

    [Url(ErrorMessage = "URL do anexo inválida")]
    public string? AttachmentUrl { get; set; }
}