using System.Collections.Generic;
using JCCommon.Clients.FileServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Scv.Api.Models.Criminal
{
    /// <summary>
    /// This is used to narrow down the amount of information from CriminalFileDetailResponse. 
    /// </summary>
    public class RedactedCriminalFileDetailResponse
    {
        [JsonProperty("responseCd", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string ResponseCd { get; set; }

        [JsonProperty("responseMessageTxt", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string ResponseMessageTxt { get; set; }

        [JsonProperty("justinNo", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string JustinNo { get; set; }

        [JsonProperty("fileNumberTxt", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string FileNumberTxt { get; set; }

        [JsonProperty("homeLocationAgenId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string HomeLocationAgenId { get; set; }

        [JsonProperty("courtLevelCd", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public CriminalFileDetailResponseCourtLevelCd CourtLevelCd { get; set; }

        [JsonProperty("courtClassCd", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public CriminalFileDetailResponseCourtClassCd CourtClassCd { get; set; }

        [JsonProperty("currentEstimateLenQty", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string CurrentEstimateLenQty { get; set; }

        [JsonProperty("currentEstimateLenUnit", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public CriminalFileDetailResponseCurrentEstimateLenUnit CurrentEstimateLenUnit { get; set; }

        [JsonProperty("initialEstimateLenQty", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string InitialEstimateLenQty { get; set; }

        [JsonProperty("initialEstimateLenUnit", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public CriminalFileDetailResponseInitialEstimateLenUnit InitialEstimateLenUnit { get; set; }

        [JsonProperty("trialStartDt", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string TrialStartDt { get; set; }

        [JsonProperty("mdocSubCategoryDsc", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string MdocSubCategoryDsc { get; set; }

        [JsonProperty("indictableYN", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public CriminalFileDetailResponseIndictableYN IndictableYN { get; set; }

        [JsonProperty("mdocCcn", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string MdocCcn { get; set; }

        [JsonProperty("participant", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<CriminalParticipant> Participant { get; set; }
    }
}
