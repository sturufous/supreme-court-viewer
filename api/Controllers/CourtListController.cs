using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scv.Api.Models.CourtList;
using Scv.Api.Services;

namespace Scv.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "BasicAuthentication, SiteMinderAuthentication")]
    [Route("api/[controller]")]
    [ApiController]
    public class CourtListController : ControllerBase
    {
        #region Variables

        private readonly CourtListService _courtListService;

        #endregion Variables


        #region Constructor

        public CourtListController(CourtListService courtListService)
        {
            _courtListService = courtListService;
        }

        #endregion Constructor

        /// <summary>
        /// Gets a court list.
        /// </summary>
        /// <param name="agencyId">Agency Identifier Code (Location Code); for example 4801 (Kelowna).</param>
        /// <param name="roomCode">The room code; for example </param>
        /// <param name="proceeding">The proceeding date in the format YYYY-MM-dd</param>
        /// <param name="divisionCode">The division code; CR, or CV.</param>
        /// <param name="fileNumber">The full file number; for example 1500-3</param>
        /// <returns>CourtList</returns>
        [HttpGet]
        [Route("court-list")]
        public async Task<ActionResult<CourtList>> GetCourtList(string agencyId, string roomCode, DateTime proceeding, string divisionCode, string fileNumber)
        {
            var courtList = await _courtListService.CourtListAsync(agencyId, roomCode, proceeding, divisionCode,
                fileNumber);
            return Ok(courtList);
        }
    }
}
