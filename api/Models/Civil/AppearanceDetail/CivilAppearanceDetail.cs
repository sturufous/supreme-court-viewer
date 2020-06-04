using JCCommon.Clients.FileServices;
using System.Collections.Generic;
using Scv.Api.Models.Civil.Appearances;

namespace Scv.Api.Models.Civil.AppearanceDetail
{
    public class CivilAppearanceDetail
    {
        public string PhysicalFileId { get; set; }
        public string AgencyId { get; set; }
        public string AppearanceId { get; set; }
        public string CourtRoomCd { get; set; }
        public string FileNumberTxt { get; set; }
        public string AppearanceDt { get; set; }
        /// <summary>
        /// Extended object. 
        /// </summary>
        public ICollection<CivilAppearanceDetailParty> Party { get; set; }

        public ICollection<ClParty> CourtListParty { get; set; }

        /// <summary>
        /// Extended document object.
        /// </summary>
        public ICollection<CivilAppearanceDocument> Document { get; set; }

        public ICollection<CivilAppearanceMethod> AppearanceMethod { get; set; }
    }
}