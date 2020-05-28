using JCCommon.Clients.FileServices;
using System;

namespace JCCommon.Models
{
    public class FilesCivilQuery
    {
        public SearchMode2 SearchMode { get; set; }
        public string FileHomeAgencyId { get; set; }
        public string FileNumber { get; set; }
        public string FilePrefix { get; set; }
        public string FilePermissions { get; set; }
        public string FileSuffixNumber { get; set; }
        public string MDocReferenceTypeCode { get; set; }
        public CourtClassCd3? CourtClass { get; set; }
        public CourtLevelCd3? CourtLevel { get; set; }
        public NameSearchTypeCd2? NameSearchType { get; set; }
        public string LastName { get; set; }
        public string OrgName { get; set; }
        public string GivenName { get; set; }
        public DateTime? Birth { get; set; }
        public string SearchByCrownPartId { get; set; }
        public SearchByCrownActiveOnlyYN2? SearchByCrownActiveOnly { get; set; }
        public SearchByCrownFileDesignationCd2? SearchByCrownFileDesignation { get; set; }
        public string MdocJustinNumberSet { get; set; }
        public string PhysicalFileIdSet { get; set; }
    }
}