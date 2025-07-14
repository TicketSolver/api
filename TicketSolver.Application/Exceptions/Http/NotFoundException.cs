using System.Net;

namespace TicketSolver.Application.Exceptions.Http;

public class NotFoundException : HttpException
{
    public NotFoundException(string message) : base(HttpStatusCode.NotFound, message)
    {
    }
}
