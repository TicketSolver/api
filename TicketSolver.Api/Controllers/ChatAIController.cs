// TicketSolver.Api/Controllers/ChatController.cs
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using GroqNet.ChatCompletions;
using TicketSolver.Api.Models;
using TicketSolver.Application.Services.ChatAI;
using TicketSolver.Application.Services.ChatAI.Interface;

namespace TicketSolver.Api.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        // guardamos em memória o histórico de cada conversa
        private static readonly ConcurrentDictionary<Guid, GroqChatHistory> _histories
            = new();

        public ChatController(IChatService chatService)
            => _chatService = chatService;

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest request, CancellationToken ct)
        {
            // 1) define ou gera um novo ID de conversa
            var convId  = request.ConversationId ?? Guid.NewGuid();

            // 2) recupera (ou inicia) o histórico desta conversa
            var history = _histories.GetOrAdd(convId, _ => new GroqChatHistory());

            // 3) adiciona a mensagem do usuário
            history.Add(new GroqMessage(request.Prompt) { Role = "user" });

            // 4) envia tudo para o serviço / GroqClient
            var reply = await _chatService.AskAsync(history, ct);

            // 5) adiciona a resposta da IA no histórico
            history.Add(new GroqMessage(reply) { Role = "assistant" });

            // 6) devolve o texto e o ID de conversa
            var response = new ChatResponse(convId, reply);
            return Ok(response);
        }
    }
}