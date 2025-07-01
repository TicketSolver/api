// TicketSolver.Application.Exceptions.Service
namespace TicketSolver.Application.Exceptions.Service;

public class ServiceException : Exception
{
    public ServiceException(string message) : base(message) { }
}

public class ServiceNotFoundException : ServiceException
{
    public ServiceNotFoundException() : base("Atendimento não encontrado.") { }
}