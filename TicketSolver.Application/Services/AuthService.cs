using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using TicketSolver.Application.Configuration;
using TicketSolver.Application.Exceptions.Users;
using TicketSolver.Application.Models.Auth;
using TicketSolver.Application.Services.Interfaces;
using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Domain.Persistence.Tables.User;
using TicketSolver.Domain.Repositories.Tenant;
using TicketSolver.Domain.Repositories.User;

namespace TicketSolver.Application.Services;

public class AuthService(
    ITenantsRepository tenantsRepository,
    IUsersRepository usersRepository,
    UserManager<Users> userManager,
    JwtSettings jwtSettings
) : IAuthService
{
    public async Task<IdentityResult> PreRegisterUserAsync(PreRegisterModel model, CancellationToken cancellationToken)
    {
        var user = new Users
        {
            Email = model.Email,
            UserName = model.Email,
            FullName = model.FullName,
            DefUserTypeId = model.DefUserTypeId,
            DefUserStatusId = model.DefUserStatusId,
            TenantId = model.TenantId
        };

        return await userManager.CreateAsync(user, model.Password);
    }

    public async Task<LoginDataReturn> LoginUserAsync(LoginModel model, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        var loginDataReturn = new LoginDataReturn();
        if (
            user == null
            || user.DefUserStatusId == (short)eDefUserStatus.Inactive
            || !await userManager.CheckPasswordAsync(user, model.Password)
        )
            throw new AuthenticationFailedException();
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtSettings.JwtKey);
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? throw new InvalidOperationException()),
                new Claim(ClaimTypes.Role, user.DefUserTypeId.ToString()),
            }),
            Expires = DateTime.UtcNow.AddHours(jwtSettings.Expiration),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var writtenToken = tokenHandler.WriteToken(token);
        loginDataReturn.Token = writtenToken;
        loginDataReturn.User = new UsersReturn(user);
        return loginDataReturn;
    }

    public async Task<Users> RegisterUserAsync(RegisterModel model,
        CancellationToken cancellationToken)
    {
        var tenant = await tenantsRepository.GetTenantByKeyAsync(model.Key, cancellationToken);
        if (tenant == null)
            throw new InvalidPublicKeyException();

        var preRegisteredUser = await usersRepository
            .GetByEmailAsync(model.Email, cancellationToken);
        
        if (preRegisteredUser is not null)
            return await FinishRegistratioAsync(preRegisteredUser, cancellationToken);

        if (tenant.AdminKey != model.Key)
            throw new UserNotFoundException();

        var user = new Users
        {
            Email = model.Email,
            UserName = model.Email,
            FullName = model.Email,
            DefUserStatusId = (short)eDefUserStatus.Active,
            DefUserTypeId = (short)eDefUserTypes.Admin,
            TenantId = tenant.Id,
        };
        user.CreatedAt = user.CreatedAt.ToUniversalTime();
        user.UpdatedAt = user.UpdatedAt.ToUniversalTime();
        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            throw new UserRegistrationException(result.Errors.Select(e => e.Description));
        return user;
    }

    public async Task<KeyModel> VerifyKeyAsync(KeyModel key, CancellationToken cancellationToken)
    {
        var tenant = await tenantsRepository.GetTenantByKeyAsync(key.TenantKey, cancellationToken);
        if (tenant is null)
            throw new KeyNotFoundException();
        
        int typeKey = await GetTypeKey(key.TenantKey, cancellationToken);
    
        switch (typeKey)
        {
            case 0:
                return new KeyModel(tenant.AdminKey,0);
            case 1:
                return new KeyModel(tenant.PublicKey, 1);
            default:
                throw new KeyNotFoundException();
        }
    }


    public Task<int> GetTypeKey(Guid key, CancellationToken cancellationToken)
    {
        return tenantsRepository.GetTypeKey(key, cancellationToken);
    }

    private async Task<Users> FinishRegistratioAsync(Users user, CancellationToken cancellationToken)
    {
        if (user.DefUserStatusId != (short)eDefUserStatus.Inactive)
            throw new UserAlreadyRegisteredException();

        user.DefUserStatusId = (short)eDefUserStatus.Active;
        await usersRepository.UpdateUserAsync(user, cancellationToken);

        return user;
    }
}

