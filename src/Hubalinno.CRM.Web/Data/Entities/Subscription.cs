using Hubalinno.CRM.Shared;

namespace Hubalinno.CRM.Web.Data.Entities;

public class Subscription
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public Account? Account { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Notes { get; set; }
}
