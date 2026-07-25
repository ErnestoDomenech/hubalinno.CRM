namespace Hubalinno.CRM.Shared.Dtos;

public class ProductDto
{
    public int Id { get; set; }
    public ProductKey Key { get; set; }
    public string Name { get; set; } = "";
    public BusinessLine BusinessLine { get; set; }
}
