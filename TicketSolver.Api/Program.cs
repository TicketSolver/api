using Microsoft.AspNetCore.Builder;
using TicketSolver.Api.Main;

var builder = WebApplication.CreateBuilder(args);
var app = Startup.Configure(builder);
app.Run();