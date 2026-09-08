using System;
using System.Collections.Generic;
using System.Text;

namespace Hubalinno.CRM.Shared
{
    public enum InvestorPipelineStage
    {
        Target = 0,
        Research = 1,
        WarmIntro = 2,
        Contacted = 3,
        Conversation = 4,
        MaterialsSent = 5,
        DueDiligence = 6,
        Investing = 7,
        Passed = 8
    }
}
