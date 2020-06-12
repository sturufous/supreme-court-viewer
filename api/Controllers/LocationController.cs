using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JCCommon.Clients.LocationServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scv.Api.Helpers.Extensions;
using Scv.Api.Models.Location;
using Scv.Api.Services;

namespace Scv.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly LocationService _locationService;

        public LocationController(IConfiguration configuration, LocationService locationService)
        {
            _locationService = locationService;
        }

        /// <summary>
        /// Provides locations and their court rooms. 
        /// </summary>
        /// <returns>List{Location}</returns>
        [HttpGet]
        [Route("locations")]
        public async Task<ActionResult<List<Location>>> GetLocationsAndCourtRooms()
        {
            var locations = await _locationService.GetLocations();

            var locationList = locations.Select(location => new Location
            {
                Name = location.LongDesc,
                Active = location.Flex?.Equals("Y"),
                LocationId = location.ShortDesc,
                Code = location.Code
            }).ToList();

            var courtRooms = await _locationService.GetCourtRooms();

            foreach (var location in locationList)
            {
                location.CourtRooms = courtRooms.Where(cr => cr.Flex == location.LocationId)
                    .Select(cr => new CourtRoom {LocationId = cr.Flex, Room = cr.Code, Type = cr.ShortDesc}).ToList();
            }

            return Ok(locationList);
        }
    }
}
