using MobileSolver.Api.Middlewares;
using MobileSolver.Api.Settings.Swagger;
using TicketSolver.Domain.Extensions;
using TicketSolver.Infra.EntityFramework.Persistence.Seeding;
using TicketSolver.Infra.Storage.Extensions;
using TicketSolver.Infra.Storage.Settings.Storage;

namespace MobileSolver.Api.Main;

public static class Startup
{
    public static WebApplication Configure(WebApplicationBuilder builder)
    {
        builder.Configuration.AddEnvironmentBasedDotEnv();
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        ConfigureDbConnection.Setup(builder.Services, builder.Configuration);
        ConfigureIdentity.Setup(builder.Services);
        ConfigureDependencies.Setup(builder.Services);
        ConfigureRepositories.Setup(builder.Services);
        ConfigureServices.Setup(builder.Services);
        builder.Services.AddActions();
        SwaggerExtensions.AddSwagger(builder);
        builder.Services.Configure<StorageSettings>(builder.Configuration.GetSection("Storage"));
        builder.Services.ConfigureStorage(builder.Configuration);
        
        builder.Services.AddTransient<SeedingService>();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend",
                builder => builder
                    .WithOrigins("http://localhost:3000")
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });
        
        
        var app = builder.Build();
        app.UseMiddleware<HttpExceptionHandler>();
        app.UseCors("AllowFrontend");
        app.UseAuthentication(); // necessário para ativar o middleware de autenticação
        app.UseAuthorization();  // necessário para os [Authorize] nos controllers
        ConfigureApp.Setup(app, builder.Environment.IsDevelopment());
        SwaggerExtensions.UseSwagger(app, builder);
        
        return app;
    }
}