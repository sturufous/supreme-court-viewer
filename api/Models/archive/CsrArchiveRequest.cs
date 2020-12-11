using System.Collections.Generic;

namespace Scv.Api.Models.archive
{
    public class CsrArchiveRequest
    {
        public string ZipName { get; set; }
        public List<CsrRequest> CSRRequest { get; set; }
    }

    public class CsrRequest
    {
        public string PdfFileName { get; set; }
        public string AppearanceId { get; set; }
    }
}
