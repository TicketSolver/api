using TicketSolver.Api.Main;
using Microsoft.EntityFrameworkCore;
using TicketSolver.Api.Data;
using TicketSolver.Api.Settings;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(DataBaseConnection.DbConnection));
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
var app = Startup.Configure(builder);
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.Run();


