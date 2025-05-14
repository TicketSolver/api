using System.Net;

namespace TicketSolver.Api.Exceptions;

public class BadRequestException(string message) : HttpException(message, HttpStatusCode.BadRequest)
{
    
}