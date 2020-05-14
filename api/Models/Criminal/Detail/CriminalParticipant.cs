using System.Collections.Generic;
using JCCommon.Clients.FileServices;
using Newtonsoft.Json;

namespace Scv.Api.Models.Criminal.Detail
{
    /// <summary>
    /// Extends JCCommon.CriminalParticipant
    /// </summary>
    public class CriminalParticipant
    {
        [JsonProperty("fullName")]
        public string FullName => $"{GivenNm} {LastNm}";

        [JsonProperty("partId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string PartId { get; set; }

        [JsonProperty("profSeqNo", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string ProfSeqNo { get; set; }

        [JsonProperty("lastNm", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string LastNm { get; set; }

        [JsonProperty("givenNm", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string GivenNm { get; set; }

        [JsonProperty("orgNm", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string OrgNm { get; set; }

        [JsonProperty("warrantYN", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public CriminalParticipantWarrantYN WarrantYN { get; set; }

        [JsonProperty("inCustodyYN", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public CriminalParticipantInCustodyYN InCustodyYN { get; set; }

        [JsonProperty("interpreterYN", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public CriminalParticipantInterpreterYN InterpreterYN { get; set; }

        [JsonProperty("detainedYN", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public CriminalParticipantDetainedYN DetainedYN { get; set; }

        [JsonProperty("birthDt", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string BirthDt { get; set; }

        [JsonProperty("counselRrepId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string CounselRrepId { get; set; }

        [JsonProperty("counselPartId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string CounselPartId { get; set; }

        [JsonProperty("counselLastNm", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string CounselLastNm { get; set; }

        [JsonProperty("counselGivenNm", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string CounselGivenNm { get; set; }

        [JsonProperty("counselRelatedRepTypeCd", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string CounselRelatedRepTypeCd { get; set; }

        [JsonProperty("counselEnteredDt", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string CounselEnteredDt { get; set; }

        [JsonProperty("designatedCounselYN", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public CriminalParticipantDesignatedCounselYN DesignatedCounselYN { get; set; }

        [JsonProperty("charge", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Charge> Charge { get; set; }
    }
}
