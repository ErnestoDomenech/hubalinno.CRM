namespace Hubalinno.CRM.Shared.Dtos;

/// <summary>Modelo de solo lectura para respuestas de la API.</summary>
public class OpportunityDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string AccountDisplayName { get; set; } = "";
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public BusinessLine BusinessLine { get; set; }

    public int? PipelineStageId { get; set; }
    public string PipelineStageKey { get; set; } = "";
    public string PipelineStageName { get; set; } = "";
    public bool IsWon { get; set; }
    public bool IsLost { get; set; }
    public bool IsOpen { get; set; }

    public decimal EstimatedValue { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public string? OwnerUserId { get; set; }
    public string? OwnerName { get; set; }

    public int? NextActionActivityId { get; set; }
    public string? NextActionSubject { get; set; }
    public DateTime? NextActionDueDate { get; set; }
    public bool IsNextActionMissing { get; set; }

    public DateTime CreatedAt { get; set; }
}
