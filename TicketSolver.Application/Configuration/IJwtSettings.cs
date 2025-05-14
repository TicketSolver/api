namespace TicketSolver.Application.Configuration;

public class IJwtSettings
{
    public string JwtKey { get; set; }
    public int Expiration { get; set; } = 12;
}