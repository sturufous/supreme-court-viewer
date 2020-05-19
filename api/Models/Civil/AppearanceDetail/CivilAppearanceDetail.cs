using System.Collections.Generic;
using JCCommon.Clients.FileServices;
using Scv.Api.Models.Civil.Detail;

namespace Scv.Api.Models.Civil.AppearanceDetail
{
    public class CivilAppearanceDetail
    {
        public string PhysicalFileId { get; set; }
        public ICollection<CivilAppearanceParty> Party { get; set; }
        /// <summary>
        /// Extended document object. 
        /// </summary>
        public ICollection<CivilAppearanceDocument> Document { get; set; }
        public ICollection<CivilAppearanceMethod> AppearanceMethod { get; set; }
    }
}
