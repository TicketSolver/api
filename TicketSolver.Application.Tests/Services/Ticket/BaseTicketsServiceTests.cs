using System.Threading.Tasks;
using Moq;
using TicketSolver.Application.Actions.Ticket.Interfaces;
using TicketSolver.Application.Models;
using TicketSolver.Application.Services.Ticket;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Ticket;
using TicketSolver.Domain.Repositories.User;
using Xunit;

namespace TicketSolver.Application.Tests.Services.Ticket;

public class BaseTicketsServiceTests
{
    private readonly Mock<ITicketsRepository<Tickets>> _ticketsRepositoryMock;
    private readonly Mock<IUsersRepository> _usersRepositoryMock;
    private readonly Mock<ITicketUsersRepository> _ticketUsersRepositoryMock;
    private readonly Mock<ICreateTicketAction<Tickets>> _createTicketActionMock;
    private readonly BaseTicketsService<Tickets> _ticketsService;

    public BaseTicketsServiceTests()
    {
        _ticketsRepositoryMock = new Mock<ITicketsRepository<Tickets>>();
        _usersRepositoryMock = new Mock<IUsersRepository>();
        _ticketUsersRepositoryMock = new Mock<ITicketUsersRepository>();
        _createTicketActionMock = new Mock<ICreateTicketAction<Tickets>>();
        _ticketsService = new BaseTicketsService<Tickets>(
            _ticketsRepositoryMock.Object,
            _usersRepositoryMock.Object,
            _ticketUsersRepositoryMock.Object,
            _createTicketActionMock.Object,
            _ticketsRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Ticket()
    {
        // Arrange
        var ticketDto = new TicketDTO
        {
            Title = "Test Ticket",
            Description = "Test Description",
            Priority = 1,
            Category = 1
        };
        var userId = "test-user";
        var createdTicket = new Tickets
        {
            Id = 1,
            Title = "Test Ticket",
            Description = "Test Description",
            CreatedById = userId
        };

        _createTicketActionMock.Setup(x => x.ExecuteAsync(It.IsAny<Tickets>(), ticketDto, default))
            .ReturnsAsync(createdTicket);

        // Act
        var result = await _ticketsService.CreateAsync(ticketDto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdTicket.Id, result.Id);
        Assert.Equal(createdTicket.Title, result.Title);
        Assert.Equal(createdTicket.Description, result.Description);
        Assert.Equal(createdTicket.CreatedById, result.CreatedById);
    }
}
