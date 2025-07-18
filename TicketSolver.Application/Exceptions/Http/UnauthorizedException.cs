using System.Net;

namespace TicketSolver.Application.Exceptions.Http;

public class UnauthorizedException : HttpException
{
    public UnauthorizedException(string message) : base(HttpStatusCode.Unauthorized, message)
    {
    }
}
