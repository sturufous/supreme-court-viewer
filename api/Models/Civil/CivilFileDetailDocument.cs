using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JCCommon.Clients.FileServices;
using Newtonsoft.Json;

namespace Scv.Api.Models.Civil
{
    /// <summary>
    /// This includes extra fields that our API doesn't give us. 
    /// </summary>
    public class CivilFileDetailDocument : CvfcDocument3
    {
        [JsonProperty("category", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Category { get; set; }
        [Newtonsoft.Json.JsonProperty("documentTypeDescription", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string DocumentTypeDescription { get; set; }
    }
}
