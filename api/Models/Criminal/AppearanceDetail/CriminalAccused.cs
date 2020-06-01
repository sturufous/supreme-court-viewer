using System.Collections.Generic;
using JCCommon.Clients.FileServices;

namespace Scv.Api.Models.Criminal.AppearanceDetail
{
    public class CriminalAccused
    {
        public string FullName { get; set; }
        public ICollection<CriminalAppearanceMethod> AppearanceMethods { get; set; }
    }
}
