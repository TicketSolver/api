using System.ComponentModel.DataAnnotations.Schema;
using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Persistence.Tables.Defs;
using TicketSolver.Domain.Persistence.Tables.User;

namespace TicketSolver.Domain.Persistence.Tables.Ticket;

public class Tickets : EntityDates
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public short Status { get; set; } = (short)eDefTicketStatus.New;
    public DateTime? SolvedAt { get; set; }
    public string CreatedById { get; set; }
    public short DefTicketPriorityId { get; set; }
    public short DefTicketCategoryId { get; set; }
    public short DefUserSatisfactionId { get; set; } = (short)eDefUserSatisfaction.Neutral;

    [ForeignKey("CreatedById")] public Users CreatedBy { get; set; }
    [ForeignKey("DefTicketPriorityId")] public DefTicketPriorities DefTicketPriority { get; set; }
    [ForeignKey("DefTicketCategoryId")] public DefTicketCategories DefTicketCategory { get; set; }
    [ForeignKey("DefUserSatisfactionId")] public DefUserSatisfaction DefUserSatisfaction { get; set; }
    
    public ICollection<TicketUpdates> TicketUpdates { get; set; } = [];
    public ICollection<TicketUsers> TicketUsers { get; set; } = [];
    public ICollection<Attachments> Attachments { get; set; } = [];
    
    public Tickets(){}

    public Tickets(Tickets other)
    {
        Id = other.Id;
        Title = other.Title;
        Description = other.Description;
        Status = other.Status;
        SolvedAt = other.SolvedAt;
        CreatedById = other.CreatedById;
        CreatedBy = other.CreatedBy;
        DefTicketPriorityId = other.DefTicketPriorityId;
        DefTicketCategoryId = other.DefTicketCategoryId;
        DefUserSatisfactionId = other.DefUserSatisfactionId;
        // TicketUpdates = other.TicketUpdates;
        // TicketUsers = other.TicketUsers;
        // Attachments = other.Attachments;
    }
}