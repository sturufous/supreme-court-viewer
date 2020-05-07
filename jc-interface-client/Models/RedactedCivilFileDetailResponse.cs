﻿using System.Collections.Generic;
using JCCommon.Clients.FileServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JCCommon.Models
{
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

        [JsonProperty("courtLevelCd", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public CivilFileDetailResponseCourtLevelCd CourtLevelCd { get; set; }

        [JsonProperty("courtClassCd", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public CivilFileDetailResponseCourtClassCd CourtClassCd { get; set; }

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

        [JsonProperty("party", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<CvfcParty3> Party { get; set; }

        [JsonProperty("document", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<CvfcDocument3> Document { get; set; }

        [JsonProperty("hearingRestriction", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<CvfcHearingRestriction2> HearingRestriction { get; set; }

    }
}
