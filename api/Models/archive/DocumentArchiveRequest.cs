using System.Collections.Generic;

namespace Scv.Api.Models.archive
{
    public class DocumentArchiveRequest
    {
        public string ZipName { get; set; }
        public bool IsCriminal { get; set; }
        public List<DocumentRequest> DocumentRequest { get; set; }
    }

    public class DocumentRequest
    {
        public string PdfFileName { get; set; }
        public string DocumentId { get; set; }
    }
}
