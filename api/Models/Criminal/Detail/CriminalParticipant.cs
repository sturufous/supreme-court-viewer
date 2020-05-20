using System.Collections.Generic;

namespace Scv.Api.Models.Criminal.Detail
{
    /// <summary>
    /// Extends JCCommon.Clients.FileServices.CriminalParticipant
    /// </summary>
    public class CriminalParticipant : JCCommon.Clients.FileServices.CriminalParticipant
    {
        public string FullName => $"{GivenNm?.Trim()} {LastNm?.Trim()}";

        /// <summary>
        /// Custom class to extend. 
        /// </summary>
        public ICollection<CriminalDocument> Document { get; set; }
    }
}
