using Hubalinno.CRM.Shared;

namespace Hubalinno.CRM.Web.Data.Entities;

public class Opportunity
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public Account? Account { get; set; }
    public int? ContactId { get; set; }
    public Contact? Contact { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    /// <summary>
    /// Etapa heredada del MVP inicial. Ya no se escribe en altas nuevas ni se expone en la API:
    /// <see cref="PipelineStageId"/> es la fuente de verdad. Se conserva la columna como
    /// artefacto de auditoría/rollback hasta que se retire en una fase posterior.
    /// </summary>
    public OpportunityStage Stage { get; set; }

    public int? PipelineStageId { get; set; }
    public PipelineStage? PipelineStage { get; set; }

    public decimal EstimatedValue { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public string? OwnerUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Próxima acción (cache derivado, ver ActivityRollupService)
    public int? NextActionActivityId { get; set; }
    public Activity? NextActionActivity { get; set; }
    public string? NextActionSubject { get; set; }
    public DateTime? NextActionDueDate { get; set; }
}
