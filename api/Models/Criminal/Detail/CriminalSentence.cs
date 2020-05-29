using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scv.Api.Models.Criminal.Detail
{
    /// <summary>
    /// Expands object.
    /// </summary>
    public class CriminalSentence : JCCommon.Clients.FileServices.CfcSentence
    {
        public string JudgesRecommendation { get; set; }
    }
}
