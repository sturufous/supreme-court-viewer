using System.Collections.Generic;

namespace Scv.Api.Models.Criminal.Detail
{
    /// <summary>
    /// Extends JCCommon.Clients.FileServices.CriminalParticipant
    /// </summary>
    public class CriminalParticipant : JCCommon.Clients.FileServices.CriminalParticipant
    {
        public string FullName => GivenNm != null && LastNm != null
            ? $"{GivenNm?.Trim()} {LastNm?.Trim()}"
            : OrgNm;

        /// <summary>
        /// Custom class to extend.
        /// </summary>
        public ICollection<CriminalDocument> Document { get; set; }

        /// <summary>
        /// Can only be set to true, cannot be set to false and have the fields reappear.
        /// </summary>
        public bool? HideJustinCounsel
        {
            get => _hideJustinCounsel;
            set
            {
                _hideJustinCounsel = value;
                if (value.HasValue && value.Value)
                {
                    CounselLastNm = null;
                    CounselGivenNm = null;
                    CounselEnteredDt = null;
                    CounselPartId = null;
                    CounselRelatedRepTypeCd = null;
                    CounselRrepId = null;
                }
            }
        }

        private bool? _hideJustinCounsel;
    }
}