namespace Hubalinno.CRM.Shared.Dtos;

public class ActivityDto
{
    public int Id { get; set; }
    public int? AccountId { get; set; }
    public string? AccountDisplayName { get; set; }
    public int? OpportunityId { get; set; }
    public ActivityType Type { get; set; }
    public string Subject { get; set; } = "";
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ActivityStatus Status { get; set; }
    public string? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }
}
