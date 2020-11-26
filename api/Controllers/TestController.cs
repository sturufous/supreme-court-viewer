using Microsoft.AspNetCore.Mvc;

namespace Scv.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        [Route("Headers")]

        public ActionResult Headers()
        {
            return Ok(Request.Headers);
        }
    }
}
