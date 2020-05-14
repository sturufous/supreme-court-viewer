using System.Collections.Generic;
using JCCommon.Clients.FileServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Scv.Api.Models.Civil.Detail
{
    /// <summary>
    /// This is used to refine information from CivilFileDetailResponse. 
    /// </summary>
    public class RedactedCivilFileDetailResponse
    {
        [JsonProperty("responseCd", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string ResponseCd { get; set; }

        [JsonProperty("responseMessageTxt", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string ResponseMessageTxt { get; set; }

        [JsonProperty("physicalFileId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string PhysicalFileId { get; set; }

        [JsonProperty("fileNumberTxt", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string FileNumberTxt { get; set; }

        [JsonProperty("homeLocationAgenId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string HomeLocationAgenId { get; set; }
        /// <summary>
        /// Additional field for Agency Identifier Code. 
        /// </summary>
        /// 
        [JsonProperty("HomeLocationAgencyCode", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string HomeLocationAgencyCode { get; set; }

        /// <summary>
        /// Additional field for Location Name. 
        /// </summary>
        [JsonProperty("HomeLocationAgencyName", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string HomeLocationAgencyName { get; set; }

        [JsonProperty("courtLevelCd", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public CivilFileDetailResponseCourtLevelCd CourtLevelCd { get; set; }

        /// <summary>
        /// Additional field for description. 
        /// </summary>
        [JsonProperty("courtLevelDescription", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string CourtLevelDescription { get; set; }

        [JsonProperty("courtClassCd", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public CivilFileDetailResponseCourtClassCd CourtClassCd { get; set; }
        /// <summary>
        /// Additional field for description. 
        /// </summary>
        [JsonProperty("courtClassDescription", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string CourtClassDescription { get; set; }

        [JsonProperty("socTxt", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string SocTxt { get; set; }

        [JsonProperty("leftRoleDsc", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string LeftRoleDsc { get; set; }

        [JsonProperty("rightRoleDsc", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string RightRoleDsc { get; set; }

        [JsonProperty("trialRemarkTxt", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string TrialRemarkTxt { get; set; }

        [JsonProperty("commentToJudgeTxt", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string CommentToJudgeTxt { get; set; }

        [JsonProperty("sheriffCommentText", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string SheriffCommentText { get; set; }

        [JsonProperty("sealedYN")]
        public string SealedYN { get; set; }

        /// <summary>
        /// Extended party object. Hides fields. 
        /// </summary>
        [JsonProperty("party", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<CivilParty> Party { get; set; }

        /// <summary>
        /// Extended document object. 
        /// </summary>
        [JsonProperty("document", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<CivilDocument> Document { get; set; }

        [JsonProperty("hearingRestriction", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<CvfcHearingRestriction2> HearingRestriction { get; set; }

        [Newtonsoft.Json.JsonProperty("appearance", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<CvfcAppearance> Appearance { get; set; }
        public string ActivityClassCd { get; set; }
    }
}