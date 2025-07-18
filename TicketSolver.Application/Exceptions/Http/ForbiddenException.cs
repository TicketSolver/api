using System.Net;

namespace TicketSolver.Application.Exceptions.Http;

public class ForbiddenException : HttpException
{
    public ForbiddenException(string message) : base(HttpStatusCode.Forbidden, message)
    {
    }
}
