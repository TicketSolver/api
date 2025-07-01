using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSolver.Domain.Persistence.Tables.Service;

public class ServiceAvailableSlots : EntityDates
{
    public int Id { get; set; }
    public int ServiceRequestId { get; set; }
    public DateTime AvailableDate { get; set; }
    public short Period { get; set; }
    public bool IsSelected { get; set; } = false;
    [ForeignKey("ServiceRequestId")] public ServiceRequest ServiceRequest { get; set; }
}
