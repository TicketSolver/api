using TicketSolver.Api.Middlewares;
using TicketSolver.Api.Settings.Swagger;
using TicketSolver.Domain.Extensions;
using TicketSolver.Infra.EntityFramework.Persistence.Seeding;

namespace TicketSolver.Api.Main;

public static class Startup
{
    public static WebApplication Configure(WebApplicationBuilder builder)
    {
        builder.Configuration.AddEnvironmentBasedDotEnv();
    
        ConfigureDbConnection.Setup(builder.Services, builder.Configuration);
        ConfigureIdentity.Setup(builder.Services);
        ConfigureDependencies.Setup(builder.Services);
        ConfigureRepositories.Setup(builder.Services);
        ConfigureServices.Setup(builder.Services);
        SwaggerExtensions.AddSwagger(builder);
        
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
        ConfigureApp.Setup(app, builder.Environment.IsDevelopment());
        SwaggerExtensions.UseSwagger(app, builder);
        
        return app;
    }
}