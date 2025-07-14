using GroqNet;
using GroqNet.ChatCompletions;
using RemoteSolver.Domain.Persistence.Entities;
using TicketSolver.Application.Ports;
using TicketSolver.Application.Services;
using TicketSolver.Application.Services.admin;
using TicketSolver.Application.Services.admin.Interfaces;
using TicketSolver.Application.Services.Chat;
using TicketSolver.Application.Services.Chat.Interfaces;
using TicketSolver.Application.Services.ChatAI;
using TicketSolver.Application.Services.ChatAI.Interface;
using TicketSolver.Application.Services.Interfaces;
using TicketSolver.Application.Services.Service;
using TicketSolver.Application.Services.Service.Interfaces;
using TicketSolver.Application.Services.Tenant;
using TicketSolver.Application.Services.Tenant.Interfaces;
using TicketSolver.Application.Services.Ticket;
using TicketSolver.Application.Services.Ticket.Interfaces;
using TicketSolver.Application.Services.User;
using TicketSolver.Application.Services.User.Interfaces;
using TicketSolver.Infra.GeminiAI;

namespace RemoteSolver.Api.Main;

public static class ConfigureServices
{
    public static void Setup(IServiceCollection services)
    {
        services.AddTransient<IAuthService, BaseAuthService>();
        services.AddTransient<IUsersService, BaseUsersService>();
        services.AddTransient<ITenantsService, BaseTenantsService>();
        services.AddTransient<ITicketsService<RemoteTickets>, BaseTicketsService<RemoteTickets>>();
        services.AddTransient<IAttachmentsService, BaseAttachmentsService<RemoteTickets>>();
        services.AddTransient<IChatService, BaseChatService<RemoteTickets>>();
        services.AddTransient<IAdminStatsService, BaseAdminStatsService>();
        services.AddHttpClient<IAiProvider, GeminiProvider>();
        services.AddTransient<IChatAiService, AiChatService>();
        services.AddTransient<IServiceRequestService, ServiceRequestService>();


        services.AddHttpClient("Groq");

        services.AddTransient<GroqClient>(sp =>
        {
            var apiKey = Environment.GetEnvironmentVariable("Ai__Groq__ApiKey")!;
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("Groq");
            var logger = sp.GetService<ILogger<GroqClient>>();

            var model = GroqModel.LLaMA3_8b;
            return new GroqClient(apiKey, model, httpClient, logger);
        });
    }
}
