namespace Scv.Api.Models.Criminal.AppearanceDetail
{
    public class JustinCounsel
    {
        public string FullName => CounselGivenNm != null && CounselLastNm != null
            ? $"{CounselGivenNm?.Trim()} {CounselLastNm?.Trim()}"
            : null;
        public string CounselLastNm { get; set; }
        public string CounselGivenNm { get; set; }
        public string CounselEnteredDt { get; set; }
        public string CounselPartId { get; set;}
        public string CounselRelatedRepTypeCd { get; set; }
        public string CounselRrepId { get; set; }
    }
}
