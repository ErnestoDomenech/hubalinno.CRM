namespace Hubalinno.CRM.Shared.Dtos;

public class BusinessLineSummaryDto
{
    public BusinessLine BusinessLine { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int ExpiringSoon { get; set; }
    public int OpenOpportunities { get; set; }
    public decimal OpenOpportunitiesValue { get; set; }
    public int OpenOpportunitiesMissingNextAction { get; set; }
}

public class DashboardSummaryDto
{
    public int TotalAccounts { get; set; }
    public int PendingActivitiesMine { get; set; }
    public List<BusinessLineSummaryDto> Lines { get; set; } = [];
}
