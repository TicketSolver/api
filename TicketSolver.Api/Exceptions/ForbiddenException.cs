using System.Net;

namespace TicketSolver.Api.Exceptions;

public class ForbiddenException(string message) : HttpException(message, HttpStatusCode.Forbidden)
{
}