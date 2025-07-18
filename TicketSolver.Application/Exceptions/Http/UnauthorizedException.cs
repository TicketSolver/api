using System.Net;

namespace TicketSolver.Api.Exceptions;

public class UnauthorizedException(string message) : HttpException(message, HttpStatusCode.Unauthorized)
{
}