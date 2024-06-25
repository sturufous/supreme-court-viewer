using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scv.Api.Models.PreDownload
{
    public class CancelPreDownloadRequest
    {
        public List<string> transferIds { get; set; }
    }
}