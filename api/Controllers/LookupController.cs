using JCCommon.Clients.LookupServices;
using Microsoft.AspNetCore.Mvc;
using Scv.Api.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scv.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        #region Variables

        private readonly LookupService _lookupService;

        #endregion Variables

        #region Constructor

        public LookupController(LookupService lookupService)
        {
            _lookupService = lookupService;
        }

        #endregion Constructor

        #region Actions

        [HttpGet]
        [Route("codes/documents")]
        public async Task<ActionResult<ICollection<LookupCode>>> GetDocumentCodes()
        {
            var lookupCodes = await _lookupService.GetDocuments();
            return Ok(lookupCodes);
        }

        #endregion Actions
    }
}