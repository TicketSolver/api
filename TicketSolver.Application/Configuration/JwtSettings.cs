namespace TicketSolver.Application.Configuration;

public class JwtSettings
{
    public string JwtKey { get; set; }
    public int Expiration { get; set; } = 12;
}