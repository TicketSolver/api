using TicketSolver.Application.Exceptions.Users;
using TicketSolver.Application.Models.Auth;
using TicketSolver.Application.Services.User.Interfaces;
using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Extensions;
using TicketSolver.Domain.Models;
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

        throw new NotImplementedException();
    }

    public Task<Users> UpdateUserAsync(string userId, Users user, AuthenticatedUser authenticatedUser, CancellationToken cancellationToken)
    {
        if (authenticatedUser.UserId != userId && authenticatedUser.DefUserType != eDefUserTypes.Admin)
            throw new AccessDeniedException();
        
        throw new NotImplementedException();
    }
}