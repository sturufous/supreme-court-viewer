using JCCommon.Clients.FileServices;

namespace Scv.Api.Models.CourtList
{
    public class ScheduledAppearance : ClScheduledAppearance
    {
        public string OutOfTownJudge { get; set;}
        public string SupplementalEquipment { get; set;}
        public string SecurityRestriction { get; set; }
        public string AppearanceReasonDesc { get; set; }
    }
}
