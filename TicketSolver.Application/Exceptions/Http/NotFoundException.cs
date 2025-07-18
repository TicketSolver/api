using System.Net;

namespace TicketSolver.Api.Exceptions;

public class NotFoundException(string message) : HttpException(message, HttpStatusCode.NotFound)
{
}