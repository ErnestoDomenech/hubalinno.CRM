using Hubalinno.CRM.Shared;

namespace Hubalinno.CRM.Web.Data.Entities;

public class Contact
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public Account? Account { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }

    // Prospección / TimeOn Sales OS
    public string? LinkedInUrl { get; set; }
    public bool IsPrimary { get; set; }
    public DecisionRole DecisionRole { get; set; } = DecisionRole.Unknown;
    public PreferredContactChannel PreferredChannel { get; set; } = PreferredContactChannel.Unknown;
    public DateTime? LastContactAt { get; set; }
}
