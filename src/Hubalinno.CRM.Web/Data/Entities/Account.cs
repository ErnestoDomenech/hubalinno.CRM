using Hubalinno.CRM.Shared;

namespace Hubalinno.CRM.Web.Data.Entities;

public class Account
{
    public int Id { get; set; }
    public AccountType AccountType { get; set; }
    public string? CompanyName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? TaxId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Notes { get; set; }
    public string? OwnerUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<Contact> Contacts { get; set; } = [];
}
