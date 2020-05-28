﻿using JCCommon.Clients.FileServices;
using System.Collections.Generic;

namespace Scv.Api.Models.Criminal.Detail
{
    /// <summary>
    /// This is used to narrow down the amount of information from CriminalFileDetailResponse.
    /// </summary>
    public class RedactedCriminalFileDetailResponse
    {
        public RedactedCriminalFileDetailResponse()
        {
            Ban = new List<CriminalBan>();
            Count = new List<CriminalCount>();
        }

        public string ResponseCd { get; set; }
        public string ResponseMessageTxt { get; set; }
        public string JustinNo { get; set; }
        public string FileNumberTxt { get; set; }
        public string HomeLocationAgenId { get; set; }
        public string HomeLocationAgencyName { get; set; }
        public string HomeLocationAgencyCode { get; set; }
        public string HomeLocationRegionName { get; set; }
        public string ActivityClassCd { get; set; }
        public CriminalFileDetailResponseCourtLevelCd CourtLevelCd { get; set; }
        public string CourtLevelDescription { get; set; }
        public CriminalFileDetailResponseCourtClassCd CourtClassCd { get; set; }
        public string CourtClassDescription { get; set; }
        public string CurrentEstimateLenQty { get; set; }
        public CriminalFileDetailResponseCurrentEstimateLenUnit CurrentEstimateLenUnit { get; set; }
        public string InitialEstimateLenQty { get; set; }
        public CriminalFileDetailResponseInitialEstimateLenUnit InitialEstimateLenUnit { get; set; }
        public string TrialStartDt { get; set; }
        public string MdocSubCategoryDsc { get; set; }
        public CriminalFileDetailResponseIndictableYN IndictableYN { get; set; }
        public string MdocCcn { get; set; }
        public string AssignedPartNm { get; set; }
        public string ApprovedByAgencyCd { get; set; }
        public string ApprovedByPartNm { get; set; }
        public string ApprovalCrownAgencyTypeCd { get; set; }
        public string CaseAgeDays { get; set; }
        public string CrownEstimateLenQty { get; set; }
        public string CrownEstimateLenDsc { get; set; }
        public CriminalFileDetailResponseCrownEstimateLenUnit? CrownEstimateLenUnit { get; set; }

        /// <summary>
        /// Custom class to extend.
        /// </summary>
        public ICollection<CriminalParticipant> Participant { get; set; }

        /// <summary>
        /// We need this for our witness page.
        /// </summary>
        public ICollection<CriminalWitness> Witness { get; set; }

        /// <summary>
        /// Extended.
        /// </summary>
        public ICollection<CrownWitness> Crown { get; set; }

        /// <summary>
        /// Extended.
        /// </summary>
        public ICollection<CriminalHearingRestriction> HearingRestriction { get; set; }

        /// <summary>
        /// Used for Crown Notes to JCM.
        /// </summary>
        public ICollection<TrialRemark> TrialRemark { get; set; }

        /// <summary>
        /// Extended, with PartId.
        /// </summary>
        public List<CriminalBan> Ban { get; set; }

        /// <summary>
        /// Slimmed down version of CfcAppearanceCount.
        /// </summary>
        public List<CriminalCount> Count { get; set; }
        public CriminalFileAppearancesResponse Appearances { get; set; }
    }
}