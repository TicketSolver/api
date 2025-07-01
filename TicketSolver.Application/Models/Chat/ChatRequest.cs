namespace TicketSolver.Application.Models.Chat;

public record ChatRequest(Guid? ConversationId, string Prompt, string? SystemPrompt);

public record ChatResponse(Guid ConversationId, string Reply);