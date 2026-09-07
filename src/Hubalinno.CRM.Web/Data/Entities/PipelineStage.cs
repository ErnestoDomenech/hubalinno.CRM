using Hubalinno.CRM.Shared;

namespace Hubalinno.CRM.Web.Data.Entities;

public class PipelineStage
{
    public int Id { get; set; }
    public BusinessLine BusinessLine { get; set; }
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsWon { get; set; }
    public bool IsLost { get; set; }

    public bool IsOpen => !IsWon && !IsLost;
}
