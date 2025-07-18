using System.Net;

namespace TicketSolver.Application.Exceptions.Http;

public class HttpException : Exception
{
    public HttpStatusCode HttpStatusCode { get; }

    public HttpException(HttpStatusCode httpStatusCode, string message) : base(message)
    {
        HttpStatusCode = httpStatusCode;
    }
}
