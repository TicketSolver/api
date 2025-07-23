
using TicketSolver.Application.interfaces;
using TicketSolver.Application.Models;
using TicketSolver.Application.Models.Chat;
using TicketSolver.Application.Ports;
using TicketSolver.Application.Services;
using TicketSolver.Application.Services.Chat.Interfaces;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Framework.Domain;

namespace TicketSolver.Application.Services;

    public class BaseChatService(
        IAiContextProvider contextProvider,
        IAiProvider aiProvider)
        : IChatService
    {
        public async Task<string> CreateTicketAsync(
            CancellationToken cancellationToken,
            TicketDTO ticketDto)
        {
            // 1) Converte a categoria do DTO para o enum ApplicationType (fallback Enterprise)
            var hasParsed = Enum.TryParse<ApplicationType>(
                ticketDto.Category.ToString(),
                ignoreCase: true,
                out var appType
            );
            var parsedType = hasParsed ? appType : ApplicationType.Enterprise;

            // 2) Cria um Tenants “fake” com apenas o ApplicationType
            var tenant = new Tenants
            {
                ApplicationType = parsedType
            };

            // 3) Obtém o contexto de AI para este tenant
            var aiContext = await contextProvider
                .GetAiContext(tenant, cancellationToken);

            // 4) Monta o prompt e gera o texto
            var prompt = $@"{aiContext.SystemPrompt}
Título: {ticketDto.Title}
Descrição: {ticketDto.Description}";

            return await aiProvider.GenerateTextAsync(prompt, cancellationToken);
        }

        public Task<ChatMessageResponseDto> SendMessageAsync(SendMessageRequestDto request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ChatHistoryResponseDto> GetChatHistoryAsync(int ticketId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ChatHistoryResponseDto> GetChatHistoryAsync(int ticketId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task MarkMessagesAsReadAsync(MarkAsReadRequestDto request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ChatSummaryDto>> GetChatsWithUnreadMessagesAsync(string userId, string userType, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ChatSummaryDto>> GetRecentChatsAsync(int limit = 10, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TicketPendingResponseDto>> GetTicketsPendingTechnicianResponseAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ChatStatisticsDto> GetChatStatisticsAsync(int ticketId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ChatResponseDto> StartChatAsync(int ticketId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CanAccessChatAsync(int ticketId, string userId, string userType, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ChatMessageDto>> SearchMessagesAsync(ChatSearchRequestDto request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetUnreadMessageCountAsync(int ticketId, string userId, string userType,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ChatParticipantDto>> GetChatParticipantsAsync(int ticketId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ChatMessageResponseDto> SendSystemMessageAsync(SendSystemMessageRequestDto request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ChatActivitySummaryDto> GetChatActivitySummaryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task ArchiveChatAsync(int ticketId, bool isArchived, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ChatInfoDto> GetChatInfoAsync(int ticketId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ChatMessageResponseDto> SendDirectMessageAsync(SendMessageRequestDto request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsTicketAssignedToTechnicianAsync(int ticketId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task TransferToTechnicianAsync(int ticketId, string userId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetAvailableTechniciansAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
