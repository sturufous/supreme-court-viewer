using JCCommon.Clients.LookupServices;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

        private readonly IConfiguration _configuration;
        private readonly ILogger<LookupController> _logger;
        private readonly IMapper _mapper;
        private readonly LookupService _lookupService;

        #endregion Variables

        #region Constructor

        public LookupController(IConfiguration configuration, ILogger<LookupController> logger, LookupService lookupService, IMapper mapper)
        {
            _configuration = configuration;
            _logger = logger;
            _lookupService = lookupService;
            _mapper = mapper;
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