using System;
using System.Collections.Generic;
using System.Text;
using JCCommon.Clients.FileServices;

namespace JCCommon.Models
{
    public class FilesCriminalQuery
    {
        public string RequestAgencyIdentifierId { get; set; }
        public string RequestPartId { get; set; }
        public string ApplicationCd { get; set; } 
        public SearchMode SearchMode { get; set; }
        public string FileHomeAgencyId { get; set; }
        public string FileNumberTxt { get; set; } 
        public string FilePrefixTxt { get; set; } 
        public string FilePermissions { get; set; }
        public string FileSuffixNo {get; set; }
        public string MdocRefTypeCode { get; set; }
        public CourtClassCd2? CourtClass { get; set; }
        public CourtLevelCd2? CourtLevel { get; set; }
        public NameSearchTypeCd? NameSearchTypeCd { get; set; }
        public string LastName { get; set;}
        public string OrgName { get; set; }
        public string GivenName { get; set; }
        public DateTime? Birth { get; set; }
        public string SearchByCrownPartId { get; set; }
        public SearchByCrownActiveOnlyYN? SearchByCrownActiveOnly { get; set; } 
        public SearchByCrownFileDesignationCd? SearchByCrownFileDesignation { get; set; }
        public string MdocJustinNoSet { get; set; }
        public string PhysicalFileIdSet { get; set; }
    }
}
