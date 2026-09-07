namespace Hubalinno.CRM.Shared.Dtos;

public class SubscriptionDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string AccountDisplayName { get; set; } = "";
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public BusinessLine BusinessLine { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Notes { get; set; }
}
