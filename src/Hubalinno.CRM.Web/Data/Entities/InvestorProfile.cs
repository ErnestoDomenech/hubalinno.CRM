using Hubalinno.CRM.Shared;

namespace Hubalinno.CRM.Web.Data.Entities
{
    public class InvestorProfile
    {
        public int Id { get; set; }

        public int AccountId { get; set; }
        public Account? Account { get; set; }

        public string? InvestmentStage { get; set; }

        public decimal? TypicalTicketMin { get; set; }
        public decimal? TypicalTicketMax { get; set; }

        public string? Thesis { get; set; }
        public string? Geography { get; set; }

        public int ThesisFit { get; set; }
        public int AccessScore { get; set; }

        public string? PortfolioConflict { get; set; }
        public string? WarmIntroRoute { get; set; }

        public RelationshipTemperature RelationshipTemperature { get; set; }

        public DateTime? LastInteraction { get; set; }

        public string? NextAction { get; set; }
        public DateTime? NextActionDate { get; set; }
    }
}
