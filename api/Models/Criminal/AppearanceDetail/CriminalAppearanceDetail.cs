using System.Collections.Generic;

namespace Scv.Api.Models.Criminal.AppearanceDetail
{
    public class CriminalAppearanceDetail
    {
        public string JustinNo { get; set; }
        public string PartId { get; set; }
        public string ProfSeqNo { get; set; }
        /// <summary>
        /// Extended CriminalAppearanceCount object. 
        /// </summary>
        public ICollection<CriminalCharges> Charges { get; set; }

        /// <summary>
        /// Extended CriminalAppearanceMethod object.
        /// </summary>
        public ICollection<CriminalAppearanceMethod> AppearanceMethods { get; set; }

        public JustinCounsel JustinCounsel { get; set; }
    }
}
