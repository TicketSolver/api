using TicketSolver.Api.Main;
using TicketSolver.Infra.EntityFramework.Persistence.Seeding;

var builder = WebApplication.CreateBuilder(args);
var app = Startup.Configure(builder);

try
{
    await using var scope = app.Services.CreateAsyncScope();
    var seedingService = scope.ServiceProvider.GetRequiredService<SeedingService>();
    await seedingService.SeedAsync();
}
catch (Exception e)
{
    // ignored
}

app.Run();