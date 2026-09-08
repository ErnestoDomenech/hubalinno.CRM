namespace Hubalinno.CRM.Shared.Dtos;

public class AccountDto
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
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Prospección / TimeOn Sales OS
    public string? Website { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? Industry { get; set; }
    public int? EmployeeCountMin { get; set; }
    public int? EmployeeCountMax { get; set; }
    public string? Province { get; set; }
    public LeadPriority? LeadPriority { get; set; }
    public LeadSource? LeadSource { get; set; }
    public string? CurrentTimeTrackingSystem { get; set; }
    public string? PainHypothesis { get; set; }
    public int? IcpScore { get; set; }
    public bool DoNotContact { get; set; }

    public string DisplayName => AccountType == AccountType.Company
        ? (CompanyName ?? "")
        : $"{FirstName} {LastName}".Trim();

    public AccountCategory AccountCategory { get; set; } = AccountCategory.Customer;
}
