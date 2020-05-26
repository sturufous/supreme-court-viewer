using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scv.Api.Models.Civil.Appearances
{
    /// <summary>
    /// Extends. 
    /// </summary>
    public class CivilAppearanceDetailParty  : JCCommon.Clients.FileServices.CivilAppearanceParty
    {
        public string FullName => GivenNm != null && LastNm != null
            ? $"{GivenNm?.Trim()} {LastNm?.Trim()}"
            : OrgNm;
        public string PartyRoleTypeDesc { get; set; }
    }
}
