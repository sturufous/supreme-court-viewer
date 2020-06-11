using System.Collections.Generic;
using System.Threading.Tasks;
using JCCommon.Clients.LocationServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scv.Api.Services;

namespace Scv.Api.Controllers
{
    public class LookupController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FilesController> _logger;
        private readonly LocationService _locationService;
        private readonly LookupService _lookupService;

        public LookupController(IConfiguration configuration, LocationService locationService, LookupService lookupService)
        {
            _locationService = locationService;
            _lookupService = lookupService;
            _configuration = configuration;
        }

        [HttpGet]
        [Route("locations")]
        public async Task<ActionResult<ICollection<CodeValue>>> GetLocation()
        {
            var locations = await _locationService.GetLocations();
            return Ok(locations);
        }
    }
}
