
using TicketSolver.Application.Models;
using TicketSolver.Application.Ports;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Framework.Domain;

namespace TicketSolver.Framework.Application
{
    public class BaseChatService : IChatService
    {
        private readonly IAiContextProvider _contextProvider;
        private readonly IAiProvider        _aiProvider;

        public BaseChatService(
            IAiContextProvider contextProvider,
            IAiProvider        aiProvider)
        {
            _contextProvider = contextProvider;
            _aiProvider      = aiProvider;
        }

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
            var aiContext = await _contextProvider
                .GetAiContext(tenant, cancellationToken);

            // 4) Monta o prompt e gera o texto
            var prompt = $@"{aiContext.SystemPrompt}
Título: {ticketDto.Title}
Descrição: {ticketDto.Description}";

            return await _aiProvider.GenerateTextAsync(prompt, cancellationToken);
        }
    }
}