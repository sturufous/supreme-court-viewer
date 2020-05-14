using JCCommon.Clients.FileServices;
using Newtonsoft.Json;

namespace Scv.Api.Models.Civil.Detail
{
    /// <summary>
    /// Extends CvfcParty3.
    /// </summary>
    public class CivilParty : CvfcParty3
    {
        [JsonProperty("roleTypeDescription", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string RoleTypeDescription { get; set; }

    }
}