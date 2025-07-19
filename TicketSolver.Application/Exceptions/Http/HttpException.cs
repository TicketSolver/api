using System.Net;

namespace TicketSolver.Api.Exceptions;

public class HttpException : Exception
{
    public HttpStatusCode HttpStatusCode { get; set; } = HttpStatusCode.InternalServerError;

    protected HttpException(string? message) : base(message)
    {
    }

    protected HttpException(string? message, HttpStatusCode statusCode) : base(message)
    {
        HttpStatusCode = statusCode;
    }
}