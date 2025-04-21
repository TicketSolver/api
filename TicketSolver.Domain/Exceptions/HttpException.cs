using System.Net;

namespace TicketSolver.Domain.Exceptions;

public abstract class HttpException(string message): Exception(message)
{
    public virtual HttpStatusCode Status => HttpStatusCode.BadRequest;
}