namespace Hubalinno.CRM.Shared.Dtos;

public class PipelineStageDto
{
    public int Id { get; set; }
    public BusinessLine BusinessLine { get; set; }
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsWon { get; set; }
    public bool IsLost { get; set; }
}
