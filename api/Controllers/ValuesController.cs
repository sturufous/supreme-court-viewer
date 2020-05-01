using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Scv.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new[] { "court schedular", "court viewer" };
        }
    }
}
