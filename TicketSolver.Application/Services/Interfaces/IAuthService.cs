using Microsoft.AspNetCore.Identity;
using TicketSolver.Application.Exceptions.Users;
using TicketSolver.Application.Models.Auth;

namespace TicketSolver.Application.Services.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Realiza o cadastro do usuário, se ele estiver pré-cadastrado
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidPublicKeyException"></exception>
    /// <exception cref="UserNotFoundException"></exception>
    Task<IdentityResult> RegisterUserAsync(RegisterModel model, CancellationToken cancellationToken);

    /// <summary>
    /// Autentica o usuário e retorna o token JWT
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="AuthenticationFailedException"></exception>
    Task<string> LoginUserAsync(LoginModel model, CancellationToken cancellationToken);
}