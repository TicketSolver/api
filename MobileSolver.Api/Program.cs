using TicketSolver.Infra.EntityFramework.Persistence.Seeding;
using DotNetEnv;
using MobileSolver.Api.Main;

Env.Load();  

var builder = WebApplication.CreateBuilder(args);

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