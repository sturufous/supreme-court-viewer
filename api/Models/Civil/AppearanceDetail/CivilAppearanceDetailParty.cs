using System.Collections.Generic;
using JCCommon.Clients.FileServices;

namespace Scv.Api.Models.Civil.AppearanceDetail
{
    public class CivilAppearanceDetailParty
    {
        public string FullName => GivenNm != null && LastNm != null
            ? $"{GivenNm?.Trim()} {LastNm?.Trim()}"
            : OrgNm;
        public string PartyId { get; set;}
        public string LastNm { get; set; }
        public string GivenNm { get; set; }
        public string OrgNm { get; set; }
        public string CourtParticipantId { get; set; }
        //The fields below this are from CourtList. 
        public string AttendanceMethodCd { get; set; }
        public string AttendanceMethodDesc { get; set; }
        ///These collections are from <see cref="ClParty"/>
        public ICollection<ClCounsel> Counsel { get; set; }
        public ICollection<ClRepresentative> Representative { get; set; }
        public ICollection<ClLegalRepresentative> LegalRepresentative { get; set; }
        public IEnumerable<ClPartyRole> PartyRole { get; set; }
    }
}
