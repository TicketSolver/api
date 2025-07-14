using System.Net;

namespace TicketSolver.Application.Exceptions.Http;

public class BadRequestException : HttpException
{
    public BadRequestException(string message) : base(HttpStatusCode.BadRequest, message)
    {
    }
}
