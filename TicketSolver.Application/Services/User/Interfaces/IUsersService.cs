using TicketSolver.Application.Models;
using TicketSolver.Application.Models.Auth;
using TicketSolver.Domain.Models;
using TicketSolver.Domain.Persistence.Tables.User;

namespace TicketSolver.Application.Services.User.Interfaces;

public interface IUsersService
{
    Task<Users> GetUserByIdAsync(string userId, CancellationToken cancellationToken);
    Task<IEnumerable<Users>> ListUsersAsync(PaginatedQuery query, CancellationToken cancellationToken);
    Task<Users> CreateUserAsync(Users user, CancellationToken cancellationToken);
    Task<bool> DeleteUserAsync(string userId, AuthenticatedUser authenticatedUser, CancellationToken cancellationToken);

    // Criar um Model para edição de user
    Task<Users> UpdateUserAsync(string userId, Users user, AuthenticatedUser authenticatedUser,
        CancellationToken cancellationToken);

    Task<PaginatedResponse<Users>> GetUsersTenantAsync(
        int tenantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken
    );
}