using Hubalinno.CRM.Shared;

namespace Hubalinno.CRM.Web.Data.Entities;

public class AccountBusinessLine
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public Account? Account { get; set; }
    public BusinessLine BusinessLine { get; set; }
    public DateTime AddedAt { get; set; }
    public DateTime? LastContactedAt { get; set; }
}
