namespace TicketSolver.Api.Models
{
    public record ChatRequest(Guid? ConversationId, string Prompt);
}

namespace TicketSolver.Api.Models
{
    public record ChatResponse(Guid ConversationId, string Reply);
}