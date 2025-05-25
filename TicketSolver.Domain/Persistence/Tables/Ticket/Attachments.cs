using System.ComponentModel.DataAnnotations.Schema;
using TicketSolver.Domain.Persistence.Tables.Defs;
using TicketSolver.Domain.Persistence.Tables.User;

namespace TicketSolver.Domain.Persistence.Tables.Ticket;

public class Attachments : EntityDates
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string Url { get; set; }
    public string Key { get; set; }
    public decimal FileSize { get; set; }
    public short DefStorageProviderId { get; set; }
    public int TicketId { get; set; }
    public string UserId { get; set; }
    
    [ForeignKey("UserId")] public Users User { get; set; }
    [ForeignKey("TicketId")] public Tickets Ticket { get; set; }
    [ForeignKey("DefStorageProviderId")] public DefStorageProviders DefStorageProvider { get; set; }
}