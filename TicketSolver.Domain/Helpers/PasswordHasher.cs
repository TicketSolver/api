namespace TicketSolver.Domain.Helpers;

using BCryptNext = BCrypt.Net.BCrypt;

public static class PasswordHasher
{
    /// <summary>
    /// Gera um hash seguro da senha, incluindo o salt.
    /// </summary>
    public static string Hash(string password)
    {
        return BCryptNext.HashPassword(password);
    }

    /// <summary>
    /// Compara a senha fornecida com o hash armazenado.
    /// </summary>
    public static bool Verify(string password, string storedHash)
    {
        return BCryptNext.Verify(password, storedHash);
    }
}