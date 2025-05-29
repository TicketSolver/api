using Microsoft.AspNetCore.Identity;
using TicketSolver.Application.Exceptions.Users;
using TicketSolver.Application.Models.Auth;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Domain.Persistence.Tables.User;

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
    Task<IdentityResult> PreRegisterUserAsync(PreRegisterModel model, CancellationToken cancellationToken);

    /// <summary>
    /// Autentica o usuário e retorna o token JWT
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="AuthenticationFailedException"></exception>
    Task<LoginDataReturn> LoginUserAsync(LoginModel model, CancellationToken cancellationToken);

    Task<Users> RegisterUserAsync(RegisterModel model, CancellationToken cancellationToken);
    Task<KeyModel> VerifyKeyAsync(KeyModel key, CancellationToken cancellationToken);
}