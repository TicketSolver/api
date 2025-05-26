namespace TicketSolver.Api.Models
{
    public record ChatRequest(Guid? ConversationId, string Prompt, string? SystemPrompt);
}

namespace TicketSolver.Api.Models
{
    public record ChatResponse(Guid ConversationId, string Reply);
}