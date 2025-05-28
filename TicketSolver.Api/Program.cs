using TicketSolver.Api.Main;
using TicketSolver.Application.Ports;
using TicketSolver.Application.Services.ChatAI;
using TicketSolver.Infra.EntityFramework.Persistence.Seeding;
using TicketSolver.Infra.GeminiAI;
using AiServiceImpl  = TicketSolver.Application.Services.ChatAI.AiChatService;
using CoreServiceImpl = TicketSolver.Application.Services.Chat.ChatService;
using DotNetEnv;
using TicketSolver.Application.Services.Chat;
using TicketSolver.Application.Services.Chat.Interfaces;
using TicketSolver.Application.Services.ChatAI.Interface;

Env.Load();  

var builder = WebApplication.CreateBuilder(args);

// registra o HttpClient antes de chamar Startup.Configure
builder.Services.AddHttpClient<IAiProvider, GeminiProvider>();
    
builder.Services.AddScoped<IChatAiService,    AiChatService>();
builder.Services.AddScoped<IChatService,       ChatService>();

// builder.Build() e configura controllers, swagger, etc
var app = Startup.Configure(builder);

// seed
try
{
    await using var scope = app.Services.CreateAsyncScope();
    var seeder = scope.ServiceProvider.GetRequiredService<SeedingService>();
    await seeder.SeedAsync();
}
catch { /* ignored */ }

app.Run();