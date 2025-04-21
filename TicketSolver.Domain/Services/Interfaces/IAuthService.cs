using Microsoft.AspNetCore.Identity;
using TicketSolver.Domain.Models.Auth;

namespace TicketSolver.Domain.Services.Interfaces;

public interface IAuthService
{
    Task<RegisterUserDto> GetRegistrationUser(string key, string email,
        CancellationToken cancellationToken);

    Task<bool> AttachIdentityUserToUserAsync(string identityUserId, int userId, CancellationToken cancellationToken);

    Task<int> GetUserIdByIdentityUserAsync(string identityUser, CancellationToken cancellationToken);
}