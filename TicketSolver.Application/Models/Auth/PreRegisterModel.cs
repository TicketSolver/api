using TicketSolver.Domain.Enums;

namespace TicketSolver.Application.Models.Auth;

public class PreRegisterModel
{
    public string Email { get; set; }
    public string FullName { get; set; }
    public string Password { get; set; }
    public short DefUserTypeId { get; set; }

    public short DefUserStatusId => (short)(DefUserTypeId == (short)eDefUserTypes.Admin
        ? eDefUserStatus.Active
        : eDefUserStatus.Inactive);
    
    public Guid Key { get; set; }
}