// TicketSolver.Api/Controllers/ChatController.cs
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using GroqNet;
using GroqNet.ChatCompletions;
using Microsoft.AspNetCore.Authorization;
using TicketSolver.Api.Models;
using TicketSolver.Application.Services.ChatAI.Interface;

namespace TicketSolver.Api.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatAiController : ShellController
    {
        private readonly IChatAiService _chatService;

        // store em memória de historiais por ConversationId
        private static readonly ConcurrentDictionary<Guid, GroqChatHistory> _histories
            = new();

        public ChatAiController(IChatAiService chatService)
            => _chatService = chatService;
        
        
        [HttpPost("ask")]
        [Authorize(Roles = "1,2,3")]       
        public async Task<IActionResult> Ask(
            [FromBody] ChatRequest request,
            CancellationToken ct
        )
        {
           if (string.IsNullOrWhiteSpace(request.Prompt))
               return BadRequest(ApiResponse.Fail("O campo 'Prompt' não pode estar vazio."));

            // 1) decide ou gera o ConversationId
            var convId = request.ConversationId ?? Guid.NewGuid();

            // 2) recupera/cria o history
            var history = _histories.GetOrAdd(convId, _ => new GroqChatHistory());

            // 3) chama o serviço
            var reply = await _chatService.AskAsync(
                history,
                request.Prompt,
                request.SystemPrompt
            );

            // 4) adiciona a resposta ao histórico
            history.Add(new GroqMessage(reply) { Role = "assistant" });

            // 5) devolve o DTO de resposta
            return Ok(ApiResponse.Ok(new ChatResponse(convId, reply)));
        }
    }
}
