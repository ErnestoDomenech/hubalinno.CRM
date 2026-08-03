namespace Hubalinno.CRM.Shared.Dtos;

public class OpportunityCreateRequest
{
    public int AccountId { get; set; }
    public int? ContactId { get; set; }
    public int ProductId { get; set; }

    /// <summary>Si se omite, se usa la primera etapa (SortOrder=0) de la línea de negocio del producto.</summary>
    public int? PipelineStageId { get; set; }

    public decimal EstimatedValue { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public string? OwnerUserId { get; set; }

    /// <summary>
    /// Próxima acción opcional. Si se informa, se crea una Activity (Type=Task, Status=Pending)
    /// vinculada a la oportunidad recién creada. No es obligatoria: una oportunidad puede quedar
    /// sin próxima acción y simplemente se marcará como incompleta.
    /// </summary>
    public string? NextActionSubject { get; set; }
    public DateTime? NextActionDueDate { get; set; }
}
