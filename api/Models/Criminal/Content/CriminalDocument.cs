using JCCommon.Clients.FileServices;

namespace Scv.Api.Models.Criminal.Content
{
    /// <summary>
    /// Wrapper for CfcDocument, adding in additional fields
    /// </summary>
    public class CriminalDocument : CfcDocument
    {
        [Newtonsoft.Json.JsonProperty("partId", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string PartId { get; set; }

        [Newtonsoft.Json.JsonProperty("category", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Category { get; set; }

        [Newtonsoft.Json.JsonProperty("documentTypeDescription", Required = Newtonsoft.Json.Required.DisallowNull,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string DocumentTypeDescription { get; set; }

        [Newtonsoft.Json.JsonProperty("hasFutureAppearance", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? HasFutureAppearance { get; set; }
    }
}
