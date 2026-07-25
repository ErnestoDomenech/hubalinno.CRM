using Hubalinno.CRM.Shared;

namespace Hubalinno.CRM.Web.Data.Entities;

public class Product
{
    public int Id { get; set; }
    public ProductKey Key { get; set; }
    public string Name { get; set; } = "";
    public BusinessLine BusinessLine { get; set; }
}
