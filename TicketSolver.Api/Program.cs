using TicketSolver.Api.Main;

var builder = WebApplication.CreateBuilder(args);
var app = Startup.Configure(builder);
app.Run();