using JCCommon.Clients.FileServices;
using System.Collections.Generic;
using Scv.Api.Models.Civil.Appearances;

namespace Scv.Api.Models.Civil.AppearanceDetail
{
    public class CivilAppearanceDetail
    {
        public string PhysicalFileId { get; set; }
        /// <summary>
        /// Extended object. 
        /// </summary>
        public ICollection<CivilAppearanceDetailParty> Party { get; set; }

        /// <summary>
        /// Extended document object.
        /// </summary>
        public ICollection<CivilAppearanceDocument> Document { get; set; }

        public ICollection<CivilAppearanceMethod> AppearanceMethod { get; set; }
    }
}