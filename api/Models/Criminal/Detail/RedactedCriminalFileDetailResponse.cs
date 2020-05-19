using System.Collections.Generic;
using JCCommon.Clients.FileServices;

namespace Scv.Api.Models.Criminal.Detail
{
    /// <summary>
    /// This is used to narrow down the amount of information from CriminalFileDetailResponse. 
    /// </summary>
    public class RedactedCriminalFileDetailResponse
    {
        public string ResponseCd { get; set; }
        public string ResponseMessageTxt { get; set; }
        public string JustinNo { get; set; }
        public string FileNumberTxt { get; set; }
        public string HomeLocationAgenId { get; set; }
        public string HomeLocationAgenName { get; set; }
        public string HomeLocationAgenCode { get; set; }
        public string HomeLocationRegionName { get; set; }
        public CriminalFileDetailResponseCourtLevelCd CourtLevelCd { get; set; }
        public string CourtLevelDsc { get; set; }
        public CriminalFileDetailResponseCourtClassCd CourtClassCd { get; set; }
        public string CourtClassDsc { get; set; }
        public string CurrentEstimateLenQty { get; set; }
        public CriminalFileDetailResponseCurrentEstimateLenUnit CurrentEstimateLenUnit { get; set; }
        public string InitialEstimateLenQty { get; set; }
        public CriminalFileDetailResponseInitialEstimateLenUnit InitialEstimateLenUnit { get; set; }
        public string TrialStartDt { get; set; }
        public string MdocSubCategoryDsc { get; set; }
        public CriminalFileDetailResponseIndictableYN IndictableYN { get; set; }
        public string MdocCcn { get; set; }

        /// <summary>
        /// Custom class to extend. 
        /// </summary>
        public ICollection<CriminalParticipant> Participant { get; set; }

        /// <summary>
        /// We need this for our witness page. 
        /// </summary>
        public ICollection<CriminalWitness> Witness { get; set; }

        public ICollection<CriminalHearingRestriction> HearingRestriction { get; set; }
    }
}
