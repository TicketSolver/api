using TicketSolver.Application.Configuration;

namespace TicketSolver.Api.Settings;

public class JwtSettings : Application.Configuration.JwtSettings
{
    public string JwtKey { get; set; }
    public int Expiration { get; set; }
    
}