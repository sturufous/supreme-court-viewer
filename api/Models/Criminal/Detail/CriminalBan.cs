using JCCommon.Clients.FileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scv.Api.Models.Criminal.Detail
{
    /// <summary>
    /// Extends CfcBan. 
    /// </summary>
    public class CriminalBan : CfcBan
    {
        public string PartId { get; set; }
    }
}
