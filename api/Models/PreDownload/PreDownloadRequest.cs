using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scv.Api.Models.PreDownload
{
    public class PreDownloadRequest
    {
        public string objGuid { get; set; }
        public string email { get; set; }
        public string filePath { get; set; }
        public string fileName { get; set; }
    }
}
