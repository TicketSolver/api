namespace TicketSolver.Application.Exceptions.Users
{
    public class AuthenticationFailedException : Exception
    {
        public string[] Errors { get; }

        public AuthenticationFailedException(string message, string[] errors)
            : base(message)
        {
            Errors = [];
        }
        

        public AuthenticationFailedException(string message, string[] errors, Exception innerException)
            : base(message, innerException)
        {
            Errors = errors ?? [];
        }
        
        public AuthenticationFailedException(string message)
            : base(message)
        {
        }

        public AuthenticationFailedException()
        {
        }
    }
}