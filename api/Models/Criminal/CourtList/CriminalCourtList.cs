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
        public ICollection<CrownWitness> Crown { get; set; }
        public new ICollection<ScheduledAppearance> ScheduledAppearance { get; set; }
        public new ICollection<HearingRestriction> HearingRestriction { get; set; }
    }
}
