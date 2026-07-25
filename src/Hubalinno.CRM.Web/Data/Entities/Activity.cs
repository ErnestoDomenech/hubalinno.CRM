using Hubalinno.CRM.Shared;

namespace Hubalinno.CRM.Web.Data.Entities;

public class Activity
{
    public int Id { get; set; }
    public int? AccountId { get; set; }
    public Account? Account { get; set; }
    public int? OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }
    public ActivityType Type { get; set; }
    public string Subject { get; set; } = "";
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ActivityStatus Status { get; set; }
    public string? AssignedToUserId { get; set; }
    public ApplicationUser? AssignedToUser { get; set; }
}
