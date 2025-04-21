using System.Net;

namespace TicketSolver.Domain.Exceptions;

public class UnauthorizedException(string message) : HttpException(message)
{
    public override HttpStatusCode Status => HttpStatusCode.Unauthorized;
}