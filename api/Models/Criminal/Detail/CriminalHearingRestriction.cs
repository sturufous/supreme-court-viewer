using JCCommon.Clients.FileServices;

namespace Scv.Api.Models.Criminal.Detail
{
    /// <summary>
    /// Extends JCCommon.Clients.FileServices.HearingRestriction2.
    /// </summary>
    public class CriminalHearingRestriction : HearingRestriction2
    {
        public string HearingRestrictionTypeDsc { get; set; }
    }
}