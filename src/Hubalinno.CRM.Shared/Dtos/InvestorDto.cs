using System;
using System.Collections.Generic;
using System.Text;

namespace Hubalinno.CRM.Shared.Dtos
{
    public class InvestorDto
    {
        public int AccountId { get; set; }
        public int InvestorProfileId { get; set; }

        public string InvestorName { get; set; } = "";

        public int? PrimaryContactId { get; set; }
        public string? PrimaryContactName { get; set; }

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

        public InvestorPipelineStage PipelineStage { get; set; }

        public DateTime? LastInteraction { get; set; }

        public string? NextAction { get; set; }
        public DateTime? NextActionDate { get; set; }
    }
}
