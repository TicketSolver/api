using System.ComponentModel.DataAnnotations.Schema;
using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Persistence.Tables.User;

namespace TicketSolver.Domain.Persistence.Tables.Service;

public class ServiceRequest : EntityDates
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string RequestedById { get; set; }
    public string? AssignedTechId { get; set; }
    public DateTime ServiceDate { get; set; }
    public short ServicePeriod { get; set; }
    public short Status { get; set; } = (short)eDefServiceStatus.Pending;
    public string? ServiceReport { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int AddressId { get; set; }
    [ForeignKey("TicketId")] public Tickets Ticket { get; set; }
    [ForeignKey("RequestedById")] public Users RequestedBy { get; set; }
    [ForeignKey("AssignedTechId")] public Users? AssignedTech { get; set; }
    [ForeignKey("AddressId")] public ServiceAddress Address { get; set; }
    public ICollection<ServiceAvailableSlots> AvailableSlots { get; set; } = [];
}



