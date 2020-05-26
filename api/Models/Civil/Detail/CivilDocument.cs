using JCCommon.Clients.FileServices;

namespace Scv.Api.Models.Civil.Detail
{
    /// <summary>
    /// This includes extra fields that our API doesn't give us.
    /// </summary>
    public class CivilDocument : CvfcDocument3
    {
        public string Category { get; set; }
        public string DocumentTypeDescription { get; set; }
    }
}