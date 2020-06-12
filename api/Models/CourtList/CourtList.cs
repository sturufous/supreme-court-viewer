using System;
using System.Collections.Generic;
using Scv.Api.Models.Civil.CourtList;
using Scv.Api.Models.Criminal.CourtList;

namespace Scv.Api.Models.CourtList
{
    public class CourtList : JCCommon.Clients.FileServices.CourtList
    {
        public new ICollection<CriminalCourtList> CriminalCourtList { get; set; }
        public new ICollection<CivilCourtList> CivilCourtList { get; set; }
    }
}
