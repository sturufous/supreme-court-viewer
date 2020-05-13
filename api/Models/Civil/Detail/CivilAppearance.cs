using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Scv.Api.Models.Civil.Detail
{
    public class CivilAppearanceDetail
    {
        [JsonProperty("physicalFileId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string PhysicalFileId { get; set; }

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

        //TODO still incomplete populating these. 
        public string EquipmentBookings { get; set; }
        public string AppearanceMethods { get; set; }
        public string OfferedDates { get; set; }
    }
}
