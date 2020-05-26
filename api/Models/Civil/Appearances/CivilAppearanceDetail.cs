namespace Scv.Api.Models.Civil.Appearances
{
    /// <summary>
    /// Extends the original object.
    /// </summary>
    public class CivilAppearanceDetail : JCCommon.Clients.FileServices.CivilAppearanceDetail
    {
        public string AppearanceReasonDsc { get; set; }
        public string AppearanceResultDsc { get; set; }
        public string AppearanceStatusDsc { get; set; }
        public string CourtLocationId { get; set; }
        public string CourtLocation { get; set; }
        public string DocumentTypeDsc { get; set; }
    }
}
