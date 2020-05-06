using System.Collections.Generic;
using System.Threading.Tasks;
using JCCommon.Clients.LookupServices;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scv.Api.Helpers.ContractResolver;
using Scv.Api.Helpers.Exceptions;

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
        private readonly LookupServiceClient _lookupClient;
        #endregion

        #region Constructor
        public LookupController(IConfiguration configuration, ILogger<LookupController> logger, LookupServiceClient lookupClient, IMapper mapper)
        {
            _configuration = configuration;
            _logger = logger;
            _lookupClient = lookupClient;
            _mapper = mapper;
            SetupLookupServicesClient();
        }
        #endregion

        #region Actions
        [Route("codes/documents")]
        public async Task<ActionResult<ICollection<LookupCode>>> GetDocumentCodes()
        {
            var lookupCodes = await _lookupClient.CodesDocumentsAsync();
            return Ok(lookupCodes);
        }
        #endregion

        #region Helpers
        private void SetupLookupServicesClient()
        {
            _lookupClient.JsonSerializerSettings.ContractResolver = new SafeContractResolver();
            _lookupClient.BaseUrl = _configuration.GetValue<string>("LookupServicesClient:Url") ?? throw new ConfigurationException($"Configuration 'LookupServicesClient:Url' is invalid or missing.");
        }
        #endregion
    }
}
