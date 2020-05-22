namespace Scv.Api.Models.Criminal.Appearances
{
    /// <summary>
    /// Extends the original object.
    /// </summary>
    public class CriminalAppearanceDetail : JCCommon.Clients.FileServices.CriminalAppearanceDetail
    {
        public string FullName => GivenNm != null && LastNm != null
            ? $"{GivenNm?.Trim()} {LastNm?.Trim()}"
            : OrgNm;
        public string AppearanceReasonDsc { get; set; }
        public string AppearanceResultDsc { get; set; }
        public string AppearanceStatusDsc { get; set; }
        public string CourtLocationId { get; set; }
        public string CourtLocation { get; set; }
    }
}
