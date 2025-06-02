using Microsoft.EntityFrameworkCore;
using TicketSolver.Application.Exceptions.Users;
using TicketSolver.Application.Models.Auth;
using TicketSolver.Application.Models.User;
using TicketSolver.Application.Services.User.Interfaces;
using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Extensions;
using TicketSolver.Domain.Models;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Persistence.Tables.User;
using TicketSolver.Domain.Repositories.User;

namespace TicketSolver.Application.Services.User;

public class UsersService(
    IUsersRepository usersRepository
) : IUsersService
{
    public async Task<Users> GetUserByIdAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            throw new UserNotFoundException();

        return user;
    }

    public async Task<IEnumerable<Users>> ListUsersAsync(PaginatedQuery query, CancellationToken cancellationToken)
    {
        var users = usersRepository
            .GetAll()
            // Filtros seriam adicionados aqui, antes do paginate
            .Paginate(query);

        return await usersRepository.ExecuteQueryAsync(users, cancellationToken);
    }

    public Task<Users> CreateUserAsync(Users user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteUserAsync(string userId, AuthenticatedUser authenticatedUser, CancellationToken cancellationToken)
    {
        if (authenticatedUser.DefUserType != eDefUserTypes.Admin)
            throw new AccessDeniedException();
        try
        {
            var user = usersRepository.GetById(userId)
                .FirstOrDefault();
            if (user is null)
                throw new UserNotFoundException();
            var status = usersRepository.DeleteAsync(cancellationToken, user);
            if (!status.Result)
                throw new Exception("Erro ao deletar usuário");
            return status;
        }
        catch (Exception e)
        {
            throw new Exception("Erro ao deletar usuário", e);
        }
    }

    public Task<Users> UpdateUserAsync(string userId, UserPatchDto user, AuthenticatedUser authenticatedUser, CancellationToken cancellationToken)
    {
        if (authenticatedUser.UserId != userId && authenticatedUser.DefUserType != eDefUserTypes.Admin)
            throw new AccessDeniedException();
        if (user is null)
            throw new ArgumentNullException(nameof(user));
        var patchuser = usersRepository.GetById(userId)
            .FirstOrDefault();
        if (patchuser is null)
            throw new UserNotFoundException();
        if (user.FullName != null) patchuser.FullName = user.FullName;
        patchuser.DefUserTypeId = (short)user.DefUserTypeId!;
        patchuser.TicketUsers = [];
        patchuser.UpdatedAt = DateTime.UtcNow;
        try
        {
            return usersRepository.UpdateUserAsync(patchuser, cancellationToken);
        }
        catch (Exception e)
        {
            throw new Exception("Erro ao atualizar usuário", e);
        }
    }

    public Task<PaginatedResponse<Users>> GetUsersTenantAsync(int tenantId, int page, int pageSize, CancellationToken ct)
    {
        var users = usersRepository
            .GetByTenantAsyc(tenantId, ct).Result;
        var totalCount = users.Count();
        var items = users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct).Result;

        return Task.FromResult(new PaginatedResponse<Users>(items, page, pageSize, totalCount));
    }
}