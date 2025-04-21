using TicketSolver.Api.Settings.Swagger;

namespace TicketSolver.Api.Main;

public static class Startup
{
    public static WebApplication Configure(WebApplicationBuilder builder)
    {
        ConfigureDbConnection.Setup(builder.Services, builder.Configuration);
        ConfigureDependencies.Setup(builder.Services);
        ConfigureRepositories.Setup(builder.Services);
        ConfigureServices.Setup(builder.Services);
        SwaggerExtensions.AddSwagger(builder);

        var app = builder.Build();
        ConfigureApp.Setup(app, builder.Environment.IsDevelopment());
        SwaggerExtensions.UseSwagger(app, builder);

        return app;
    }
}