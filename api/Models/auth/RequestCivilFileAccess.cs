using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scv.Api.Models.auth
{
    public class RequestCivilFileAccess
    {
        public string UserId { get; set; }
        public string FileId { get; set; }
    }
}
