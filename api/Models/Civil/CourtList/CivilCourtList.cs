using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JCCommon.Clients.FileServices;
using Scv.Api.Models.CourtList;

namespace Scv.Api.Models.Civil.CourtList
{
    public class CivilCourtList : ClCivilCourtList
    {
        public new ICollection<ScheduledAppearance> ScheduledAppearance { get; set; }
        public new ICollection<HearingRestriction> HearingRestriction { get; set; }
    }
}
