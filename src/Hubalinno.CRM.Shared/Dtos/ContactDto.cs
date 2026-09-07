namespace Hubalinno.CRM.Shared.Dtos;

public class ContactDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }

    // Prospección / TimeOn Sales OS
    public string? LinkedInUrl { get; set; }
    public bool IsPrimary { get; set; }
    public DecisionRole DecisionRole { get; set; }
    public PreferredContactChannel PreferredChannel { get; set; }

    /// <summary>Solo lectura: cache derivado, mantenido por ActivityRollupService.</summary>
    public DateTime? LastContactAt { get; set; }
}
