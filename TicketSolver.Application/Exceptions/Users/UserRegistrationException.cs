namespace TicketSolver.Application.Exceptions.Users;

public class UserRegistrationException : Exception
{
    public IEnumerable<string> Errors { get; set; }

    public UserRegistrationException(IEnumerable<string> errors)
    {
        Errors = errors;
    }

    public UserRegistrationException(string? message, IEnumerable<string> errors) : base(message)
    {
        Errors = errors;
    }
}