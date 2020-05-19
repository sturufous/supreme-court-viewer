using JCCommon.Clients.FileServices;

namespace Scv.Api.Models.Civil.Detail
{
    /// <summary>
    /// Extends CvfcParty3.
    /// </summary>
    public class CivilParty : CvfcParty3
    {
        public string RoleTypeDescription { get; set; }
    }
}