using TicketSolver.Domain.Repositories.User.Interfaces;
using TicketSolver.Domain.Services.User.Interfaces;

namespace TicketSolver.Domain.Services.User;

public class UsersService(
    IUsersRepository usersRepository
) : IUsersService
{
}