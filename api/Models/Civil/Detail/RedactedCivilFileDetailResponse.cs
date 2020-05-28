using JCCommon.Clients.FileServices;
using System.Collections.Generic;

namespace Scv.Api.Models.Civil.Detail
{
    /// <summary>
    /// This is used to refine information from CivilFileDetailResponse.
    /// </summary>
    public class RedactedCivilFileDetailResponse
    {
        public string ResponseCd { get; set; }
        public string ResponseMessageTxt { get; set; }
        public string PhysicalFileId { get; set; }
        public string FileNumberTxt { get; set; }
        public string HomeLocationAgenId { get; set; }
        public string HomeLocationAgencyName { get; set; }
        public string HomeLocationAgencyCode { get; set; }
        public string HomeLocationRegionName { get; set; }
        public string ActivityClassCd { get; set; }
        public CivilFileDetailResponseCourtLevelCd CourtLevelCd { get; set; }
        public string CourtLevelDescription { get; set; }
        public CivilFileDetailResponseCourtClassCd CourtClassCd { get; set; }
        public string CourtClassDescription { get; set; }
        public string SocTxt { get; set; }
        public string LeftRoleDsc { get; set; }
        public string RightRoleDsc { get; set; }
        public string TrialRemarkTxt { get; set; }
        public string CommentToJudgeTxt { get; set; }
        public string SheriffCommentText { get; set; }
        public string SealedYN { get; set; }

        /// <summary>
        /// Extended party object. Hides fields.
        /// </summary>
        public ICollection<CivilParty> Party { get; set; }

        /// <summary>
        /// Extended document object.
        /// </summary>
        public ICollection<CivilDocument> Document { get; set; }
        /// <summary>
        /// Extended hearing restriction object. 
        /// </summary>
        public ICollection<CivilHearingRestriction> HearingRestriction { get; set; }
    }
}