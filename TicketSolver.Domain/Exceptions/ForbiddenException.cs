using System.Net;

namespace TicketSolver.Domain.Exceptions;

public class ForbiddenException(string message) : HttpException(message)
{
    public override HttpStatusCode Status => HttpStatusCode.Forbidden;
}