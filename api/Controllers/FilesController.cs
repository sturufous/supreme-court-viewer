using JCCommon.Clients.FileServices;
using JCCommon.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scv.Api.Constants;
using Scv.Api.Helpers.Exceptions;
using Scv.Api.Models.Civil.Detail;
using Scv.Api.Models.Criminal.Detail;
using Scv.Api.Services;
using System;
using System.Threading.Tasks;
using CivilAppearanceDetail = Scv.Api.Models.Civil.AppearanceDetail.CivilAppearanceDetail;
using CriminalAppearanceDetail = Scv.Api.Models.Criminal.AppearanceDetail.CriminalAppearanceDetail;

namespace Scv.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        #region Variables

        private readonly IConfiguration _configuration;
        private readonly ILogger<FilesController> _logger;
        private readonly FilesService _filesService;

        #endregion Variables

        #region Constructor

        public FilesController(IConfiguration configuration, ILogger<FilesController> logger, FilesService filesService)
        {
            _configuration = configuration;
            _logger = logger;
            _filesService = filesService;
        }

        #endregion Constructor

        #region Actions

        #region Civil Only

        /// <summary>
        /// Provides facilities for performing a civil file search.
        /// </summary>
        /// <param name="fcq">FileCivilQuery object with many parameters - Search Modes: FILENO = 0, PARTNAME = 1, CROWN = 2, JUSTINNO = 3, PHYSID = 4</param>
        /// 
        /// <returns>FileSearchResponse</returns>
        [HttpPost]
        [Route("civil/search")]
        public async Task<ActionResult<FileSearchResponse>> FilesCivilSearchAsync(FilesCivilQuery fcq)
        {
            var fileSearchResponse = await _filesService.CivilSearchAsync(fcq);
            return Ok(fileSearchResponse);
        }

        /// <summary>
        /// Gets the details for a given civil file id.
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns>RedactedCivilFileDetailResponse</returns>
        [HttpGet]
        [Route("civil/{fileId}")]
        public async Task<ActionResult<RedactedCivilFileDetailResponse>> GetCivilFileDetailByFileId(string fileId)
        {
            var civilFileDetailResponse = await _filesService.CivilFileIdAsync(fileId);
            if (civilFileDetailResponse?.PhysicalFileId == null)
                throw new NotFoundException("Couldn't find civil file with this id.");
            return Ok(civilFileDetailResponse);
        }

        /// <summary>
        /// Gets detailed information regarding an appearance given civil file id and appearance id.
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="appearanceId"></param>
        /// <returns>CivilAppearanceDetail</returns>
        [HttpGet]
        [Route("civil/{fileId}/appearance-detail/{appearanceId}")]
        public async Task<ActionResult<CivilAppearanceDetail>> GetCivilAppearanceDetails(string fileId, string appearanceId)
        {
            var civilAppearanceDetail = await _filesService.CivilDetailedAppearanceAsync(fileId, appearanceId);
            if (civilAppearanceDetail == null)
                throw new NotFoundException("Couldn't find appearance detail with the provided file id and appearance id.");
            return Ok(civilAppearanceDetail);
        }

        /// <summary>
        /// Gets court summary report for a given appearance id.
        /// </summary>
        /// <param name="appearanceId"></param>
        /// <param name="fileNameAndExtension"></param>
        /// <returns>JustinReportResponse</returns>
        [HttpGet]
        [Route("civil/court-summary-report/{appearanceId}/{fileNameAndExtension}")]
        public async Task<IActionResult> GetCivilCourtSummaryReport(string appearanceId, string fileNameAndExtension)
        {
            var justinReportResponse = await _filesService.CivilCourtSummaryReportAsync(appearanceId, JustinReportName.CEISR035);

            if (justinReportResponse.ReportContent == null || justinReportResponse.ReportContent.Length <= 0)
                throw new NotFoundException("Couldn't find CSR with this appearance id.");

            return BuildFileResponse(justinReportResponse.ReportContent);
        }

        /// <summary>
        /// Gets the civil file content.
        /// </summary>
        /// <param name="agencyId"></param>
        /// <param name="roomCode"></param>
        /// <param name="proceeding">DateTime? converted to yyyy-MM-dd string</param>
        /// <param name="appearanceId"></param>
        /// <param name="physicalFileId"></param>
        /// <returns>CivilFileContent</returns>
        [HttpGet]
        [Route("civil/file-content")]
        public async Task<ActionResult<CivilFileContent>> GetCivilFileContent(string agencyId = null, string roomCode = null, DateTime? proceeding = null, string appearanceId = null, string physicalFileId = "")
        {
            var civilFileContent = await _filesService.CivilFileContentAsync(agencyId, roomCode, proceeding, appearanceId, physicalFileId);
            return Ok(civilFileContent);
        }

        #endregion Civil Only

        #region Criminal Only

        /// <summary>
        /// Provides facilities for performing a criminal file search.
        /// </summary>
        /// <param name="fcq">FileCriminalQuery, composed of parameters for search. </param>
        /// <returns>FileSearchResponse</returns>
        [HttpPost]
        [Route("criminal/search")]
        public async Task<ActionResult<FileSearchResponse>> FilesCriminalSearchAsync(FilesCriminalQuery fcq)
        {
            var fileSearchResponse = await _filesService.CriminalSearchAsync(fcq);
            return Ok(fileSearchResponse);
        }

        /// <summary>
        /// Get details for a given file id.
        /// </summary>
        /// <param name="fileId">Target file id.</param>
        /// <returns>CriminalFileDetailResponse</returns>
        [HttpGet]
        [Route("criminal/{fileId}")]
        public async Task<ActionResult<RedactedCriminalFileDetailResponse>> GetCriminalFileDetailByFileId(string fileId)
        {
            var redactedCriminalFileDetailResponse = await _filesService.CriminalFileIdAsync(fileId);
            if (redactedCriminalFileDetailResponse?.JustinNo == null)
                throw new NotFoundException("Couldn't find criminal file with this id.");
            return Ok(redactedCriminalFileDetailResponse);
        }

        /// <summary>
        /// Gets detailed information regarding an appearance given criminal file id, appearance id, participant id.
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="appearanceId"></param>
        /// <param name="partId"></param>
        /// <returns>CriminalAppearanceDetail</returns>
        [HttpGet]
        [Route("criminal/{fileId}/appearance-detail/{appearanceId}/{partId}")]
        public async Task<ActionResult<CriminalAppearanceDetail>> GetCriminalAppearanceDetails(string fileId, string appearanceId, string partId)
        {
            var appearanceDetail = await _filesService.CriminalAppearanceDetailAsync(fileId, appearanceId, partId);
            if (appearanceDetail == null)
                throw new NotFoundException("Couldn't find appearance details with the provided parameters.");
            return Ok(appearanceDetail);
        }

        /// <summary>
        /// Gets the criminal file content.
        /// </summary>
        /// <param name="agencyId"></param>
        /// <param name="roomCode"></param>
        /// <param name="proceeding">The proceeding date in the format YYYY-MM-dd</param>
        /// <param name="appearanceId"></param>
        /// <param name="justinNumber"></param>
        /// <returns>CriminalFileContent</returns>
        [HttpGet]
        [Route("criminal/file-content")]
        public async Task<ActionResult<CriminalFileContent>> GetCriminalFileContent(string agencyId = null, string roomCode = null, DateTime? proceeding = null, string appearanceId = null, string justinNumber = null)
        {
            var criminalFileContent = await _filesService.CriminalFileContentAsync(agencyId, roomCode, proceeding, appearanceId, justinNumber);
            return Ok(criminalFileContent);
        }

        /// <summary>
        /// Gets records of proceedings.
        /// </summary>
        /// <param name="partId">The participant id associated to the Record Of Proceedings.</param>
        /// <param name="fileNameAndExtension"></param>
        /// <param name="profSequenceNumber"></param>
        /// <param name="courtLevelCode">The associated court level code. P = 0, S = 1, A = 2: Provincial, Supreme, All</param>
        /// <param name="courtClassCode">The associated court class code. A = 0, Y = 1, T = 2, F = 3, C = 4, M = 5, L = 6, R = 7, B = 8, D = 9, E = 10, G = 11, H = 12, N = 13, O = 14, P = 15, S = 16, V = 17</param>
        /// <returns>FileContentResult</returns>
        [HttpGet]
        [Route("criminal/record-of-proceedings/{partId}/{fileNameAndExtension}")]
        public async Task<IActionResult> GetRecordsOfProceeding(string partId, string fileNameAndExtension, string profSequenceNumber, CourtLevelCd courtLevelCode, CourtClassCd courtClassCode)
        {
            var recordsOfProceeding = await _filesService.RecordOfProceedingsAsync(partId, profSequenceNumber, courtLevelCode, courtClassCode);

            if (recordsOfProceeding.B64Content == null || recordsOfProceeding.B64Content.Length <= 0)
                throw new NotFoundException("Couldn't find ROP with this part id.");

            return BuildFileResponse(recordsOfProceeding.B64Content);
        }

        #endregion Criminal Only

        /// <summary>
        /// Gets a court list.
        /// </summary>
        /// <param name="agencyId">Agency Identifier Code (Location Code); for example 4801 (Kelona).</param>
        /// <param name="roomCode">The room code; for example </param>
        /// <param name="proceeding">The proceeding date in the format YYYY-MM-dd</param>
        /// <param name="divisionCode">The division code; CR, or CV.</param>
        /// <param name="fileNumber">The full file number; for example 1500-3</param>
        /// <returns>CourtList</returns>
        [HttpGet]
        [Route("court-list")]
        public async Task<ActionResult<CourtList>> GetCourtList(string agencyId, string roomCode, DateTime? proceeding, string divisionCode, string fileNumber)
        {
            var courtList = await _filesService.CourtListAsync(agencyId, roomCode, proceeding, divisionCode,
                fileNumber);
            return Ok(courtList);
        }

        /// <summary>
        /// Gets a document.
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="fileNameAndExtension">Name of the file and extension.</param>
        /// <param name="isCriminal">True if Criminal, False if Civil</param>
        /// <returns>DocumentResponse</returns>
        [HttpGet]
        [Route("document/{documentId}/{fileNameAndExtension}")]
        public async Task<IActionResult> GetDocument(string documentId, string fileNameAndExtension, bool isCriminal = false)
        {
            var documentResponse = await _filesService.DocumentAsync(documentId, isCriminal);

            if (documentResponse.B64Content == null || documentResponse.B64Content.Length <= 0)
                throw new NotFoundException("Couldn't find document with this id.");

            return BuildFileResponse(documentResponse.B64Content);
        }

        #region Helpers

        private FileContentResult BuildFileResponse(string content)
        {
            Response.Headers.Add("X-Content-Type-Options", "nosniff");
            return File(Convert.FromBase64String(content), "application/pdf");
        }

        #endregion Helpers

        #endregion Actions
    }
}