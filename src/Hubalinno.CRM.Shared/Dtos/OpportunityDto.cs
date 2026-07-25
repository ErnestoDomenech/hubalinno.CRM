namespace Hubalinno.CRM.Shared.Dtos;

public class OpportunityDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string AccountDisplayName { get; set; } = "";
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public BusinessLine BusinessLine { get; set; }
    public OpportunityStage Stage { get; set; }
    public decimal EstimatedValue { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
