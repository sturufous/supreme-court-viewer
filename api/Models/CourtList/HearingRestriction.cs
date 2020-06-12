using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JCCommon.Clients.FileServices;

namespace Scv.Api.Models.CourtList
{
    public class HearingRestriction : ClHearingRestriction
    {
        public string HearingRestrictionTypeDesc { get; set; }
    }
}
