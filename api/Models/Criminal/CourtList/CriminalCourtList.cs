using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JCCommon.Clients.FileServices;
using Scv.Api.Models.CourtList;
using Scv.Api.Models.Criminal.Detail;

namespace Scv.Api.Models.Criminal.CourtList
{
    /// <summary>
    /// Extends CLCriminalCourtList.
    /// </summary>
    public class CriminalCourtList : ClCriminalCourtList
    {
        public string EstimatedTimeHour { get; set; }
        public string EstimatedTimeMin { get; set; }
        public string ActivityClassCd { get; set; }
        public string ActivityClassDesc { get; set; }
        public string OutOfTownJudge { get; set; }
        public string SupplementalEquipment { get; set; }
        public string SecurityRestriction { get; set; }
        public string AppearanceReasonCd { get; set; }
        public string AppearanceReasonDesc { get; set; }
        public string JudgeInitials { get; internal set; }
        public ICollection<CrownWitness> Crown { get; set; }
        public new ICollection<ScheduledAppearance> ScheduledAppearance { get; set; }
        public new ICollection<HearingRestriction> HearingRestriction { get; set; }
    }
}
