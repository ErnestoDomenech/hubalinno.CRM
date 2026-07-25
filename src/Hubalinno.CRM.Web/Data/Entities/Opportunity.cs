using Hubalinno.CRM.Shared;

namespace Hubalinno.CRM.Web.Data.Entities;

public class Opportunity
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public Account? Account { get; set; }
    public int? ContactId { get; set; }
    public Contact? Contact { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public OpportunityStage Stage { get; set; }
    public decimal EstimatedValue { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public string? OwnerUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
