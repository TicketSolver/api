using TicketSolver.Application.Interfaces.Services;
using TicketSolver.Domain.Enums;

namespace TicketSolver.Application.Interfaces.Factories;

public interface IFileStorageServiceFactory
{
    IFileStorageService Create(eDefStorageProviders provider);
}