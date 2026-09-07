namespace Hubalinno.CRM.Shared.Dtos;

/// <summary>
/// No incluye AccountId/ProductId (no se reasignan tras la creación) ni los campos de próxima
/// acción (son un cache derivado de las Activity vinculadas, no editable directamente).
/// </summary>
public class OpportunityUpdateRequest
{
    public int? ContactId { get; set; }
    public int PipelineStageId { get; set; }
    public decimal EstimatedValue { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public string? OwnerUserId { get; set; }
}
