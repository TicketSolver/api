using TicketSolver.Application.Configuration;

namespace TicketSolver.Api.Settings;

public class JwtSettings : IJwtSettings
{
    public string JwtKey { get; set; }
    public int Expiration { get; set; }
    
}