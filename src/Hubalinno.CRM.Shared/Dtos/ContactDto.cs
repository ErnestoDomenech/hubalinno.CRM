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
}
