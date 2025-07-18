// Startup.cs
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TicketSolver.Api.Middlewares;
using TicketSolver.Api.Settings.Swagger;
using TicketSolver.Domain.Extensions;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts.Interfaces;
using TicketSolver.Infra.EntityFramework.Persistence.Seeding;
using TicketSolver.Infra.Storage.Extensions;
using TicketSolver.Infra.Storage.Settings.Storage;

namespace TicketSolver.Api.Main;

public static class Startup
{
    public static WebApplication Configure(WebApplicationBuilder builder)
    {
        // 1) Carrega variáveis de ambiente
        builder.Configuration.AddEnvironmentBasedDotEnv();
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        // 2) Registra o DbContext e a interface IEfContext
        builder.Services.AddDbContext<EfContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                npgsqlOpts => npgsqlOpts.MigrationsAssembly(typeof(EfContext).Assembly.FullName)
            )
        );
        builder.Services.AddScoped<IEfContext, EfContext>();

        // 3) Identity, DI genéricas, repositórios, serviços e actions
        ConfigureIdentity.Setup(builder.Services);
        ConfigureDependencies.Setup(builder.Services);
        ConfigureRepositories.Setup(builder.Services);
        ConfigureServices.Setup(builder.Services);
        builder.Services.AddActions();

        // 4) Swagger
        SwaggerExtensions.AddSwagger(builder);

        // 5) Configuração de storage
        builder.Services.Configure<StorageSettings>(
            builder.Configuration.GetSection("Storage")
        );
        builder.Services.ConfigureStorage(builder.Configuration);

        // 6) Seeding, CORS, Middlewares
        builder.Services.AddTransient<SeedingService>();
        builder.Services.AddCors(opts =>
            opts.AddPolicy("AllowFrontend", policy =>
                policy
                    .WithOrigins("http://localhost:3000")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
            )
        );

        var app = builder.Build();

        app.UseMiddleware<HttpExceptionHandler>();
        app.UseCors("AllowFrontend");
        app.UseAuthentication();
        app.UseAuthorization();

        ConfigureApp.Setup(app, builder.Environment.IsDevelopment());
        SwaggerExtensions.UseSwagger(app, builder);

        return app;
    }
}
