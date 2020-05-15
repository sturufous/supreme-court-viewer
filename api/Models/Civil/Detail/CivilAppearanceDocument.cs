using System.Collections.Generic;
using JCCommon.Clients.FileServices;
using Mapster;
using Newtonsoft.Json;

namespace Scv.Api.Models.Civil.Detail
{
    /// <summary>
    /// This includes extra fields that our API doesn't give us.
    /// This excludes appearances, because this is used under the context of looking up an appearance details (meaning you already have the appearance to target).
    /// </summary>
    public class CivilAppearanceDocument : CvfcDocument3
    {
        [JsonProperty("category", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Category { get; set; }
        [JsonProperty("documentTypeDescription", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string DocumentTypeDescription { get; set; }

        /// <summary>
        /// Exclude this property, even though it exists in CvfcDocument3. 
        /// </summary>
        [JsonIgnore]
        [AdaptIgnore]
        public new ICollection<CvfcAppearance> Appearance { get; }
    }
}
