using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TicketSolver.Application.Configuration;
using TicketSolver.Application.Exceptions.Users;
using TicketSolver.Application.Models.Auth;
using TicketSolver.Application.Services.Interfaces;
using TicketSolver.Domain.Persistence.Tables.User;
using TicketSolver.Domain.Repositories.Tenant;
using TicketSolver.Domain.Repositories.User;

namespace TicketSolver.Application.Services;

public class AuthService(
    ITenantsRepository tenantsRepository,
    IUsersRepository usersRepository,
    UserManager<Users> userManager,
    IJwtSettings jwtSettings
) : IAuthService
{
    public async Task<IdentityResult> RegisterUserAsync(RegisterModel model, CancellationToken cancellationToken)
    {
        var user = await GetRegistrationUser(model.Key, model.Email, cancellationToken);
        return await userManager.CreateAsync(user, model.Password);
    }
    
    public async Task<string> LoginUserAsync(LoginModel model, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, model.Password))
            throw new AuthenticationFailedException();

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtSettings.JwtKey);
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.DefUserTypeId.ToString()),
            }),
            Expires = DateTime.UtcNow.AddHours(jwtSettings.Expiration),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        
        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }
    
    /// <summary>
    /// Verifica se o usuário foi pré-cadastrado pelo Admin e retorna o objeto User
    /// </summary>
    /// <param name="key"></param>
    /// <param name="email"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidPublicKeyException"></exception>
    /// <exception cref="UserNotFoundException"></exception>
    private async Task<Users> GetRegistrationUser(Guid key, string email,
        CancellationToken cancellationToken)
    {
        var tenant = await tenantsRepository.GetTenantByKeyAsync(key, cancellationToken);
        if (tenant == null)
            throw new InvalidPublicKeyException();

        var preRegisteredUser = await usersRepository
            .GetByEmailAsync(email, cancellationToken);
        
        if (preRegisteredUser is null)
            throw new UserNotFoundException();

        return preRegisteredUser;
    }
}