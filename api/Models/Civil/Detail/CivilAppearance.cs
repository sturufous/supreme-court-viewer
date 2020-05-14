using System.Collections.Generic;
using JCCommon.Clients.FileServices;
using Newtonsoft.Json;

namespace Scv.Api.Models.Civil.Detail
{
    public class CivilAppearanceDetail
    {
        [JsonProperty("physicalFileId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string PhysicalFileId { get; set; }

        [JsonProperty("party", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<CivilAppearanceParty> Party { get; set; }

        /// <summary>
        /// Extended document object. 
        /// </summary>
        [JsonProperty("document", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<CivilDocument> Document { get; set; }

        public ICollection<CivilAppearanceMethod> AppearanceMethod { get; set; }

        //OfferedDates requires database access. 
        public string OfferedDates { get; set; }
    }
}
