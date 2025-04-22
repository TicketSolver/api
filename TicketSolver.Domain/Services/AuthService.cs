using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Exceptions;
using TicketSolver.Domain.Models.Auth;
using TicketSolver.Domain.Repositories.Tenant.Interfaces;
using TicketSolver.Domain.Repositories.User.Interfaces;
using TicketSolver.Domain.Services.Interfaces;

namespace TicketSolver.Domain.Services;

public class AuthService(
    ITenantsRepository tenantsRepository,
    IUsersRepository usersRepository
) : IAuthService
{
    public async Task<RegisterUserDto> GetRegistrationUser(string key, string email,
        CancellationToken cancellationToken)
    {
        var tenant = await tenantsRepository.GetTenantByKeyAsync(key, cancellationToken);
        if (tenant == null)
            throw new UnauthorizedException("Chave inválida");

        var preRegisteredUser = await usersRepository
            .GetByEmail(email)
            .FirstOrDefaultAsync(cancellationToken);

        if (preRegisteredUser is null)
            throw new ForbiddenException("Usuário não registrado!");

        return new (){
            IdentityUser = new () { UserName = email, Email = email },
            UserId = preRegisteredUser.Id,
        };
    }

    public async Task<bool> AttachIdentityUserToUserAsync(string identityUserId, int userId, CancellationToken cancellationToken)
    {
        var updatedUser = await usersRepository.GetById(userId)
            .ExecuteUpdateAsync(setter => 
                setter.SetProperty(u => u.IdentityUserId, identityUserId),
                cancellationToken
            );

        return updatedUser > 0;
    }

    public async Task<int> GetUserIdByIdentityUserAsync(string identityUser, CancellationToken cancellationToken)
    {
        return await usersRepository.GetByIdentityUserId(identityUser)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}