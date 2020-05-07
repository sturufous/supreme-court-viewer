using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JCCommon.Clients.FileServices;
using JCCommon.Models;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scv.Api.Helpers.ContractResolver;
using Scv.Api.Helpers.Exceptions;
using Scv.Api.Models;

namespace Scv.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        #region Variables
        private readonly IConfiguration _configuration;
        private readonly ILogger<FilesController> _logger;
        private readonly IMapper _mapper;
        private readonly FileServicesClient _fsClient;
        private string _requestApplicationCode;
        private string _requestAgencyIdentifierId;
        private string _requestPartId;
        #endregion

        #region Constructor
        public FilesController(IConfiguration configuration, ILogger<FilesController> logger, FileServicesClient fsClient, IMapper mapper)
        {
            _configuration = configuration;
            _logger = logger;
            _fsClient = fsClient;
            _mapper = mapper;
            SetupFileServicesClient();
        }
        #endregion

        #region Actions

        #region Civil Only
        /// <summary>
        /// Provides facilities for performing a civil file search. 
        /// </summary>
        /// <param name="fcq">FileCivilQuery object with many parameters</param>
        /// <returns>FileSearchResponse</returns>
        [HttpPost]
        [Route("civil/search")]
        public async Task<ActionResult<FileSearchResponse>> FilesCivilSearchAsync(FilesCivilQuery fcq)
        {
            fcq.FilePermissions =
                "[\"A\", \"Y\", \"T\", \"F\", \"C\", \"M\", \"L\", \"R\", \"B\", \"D\", \"E\", \"G\", \"H\", \"N\", \"O\", \"P\", \"S\", \"V\"]"; // for now, use all types - TODO: determine proper list of types?
            var fileSearchResponse = await _fsClient.FilesCivilAsync(_requestAgencyIdentifierId, _requestPartId,
                _requestApplicationCode, fcq.SearchMode, fcq.FileHomeAgencyId, fcq.FileNumber, fcq.FilePrefix,
                fcq.FilePermissions, fcq.FileSuffixNumber, fcq.MDocReferenceTypeCode, fcq.CourtClass, fcq.CourtLevel,
                fcq.NameSearchType, fcq.LastName, fcq.OrgName, fcq.GivenName, fcq.Birth?.ToString("yyyy-MM-dd"),
                fcq.SearchByCrownPartId, fcq.SearchByCrownActiveOnly, fcq.SearchByCrownFileDesignation,
                fcq.MdocJustinNumberSet, fcq.PhysicalFileIdSet);
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
            var civilFileDetailResponse = await _fsClient.FilesCivilFileIdAsync(_requestAgencyIdentifierId, _requestPartId, fileId);
            //Add in CSRs. 
            foreach (var appearance in civilFileDetailResponse.Appearance)
            {
                civilFileDetailResponse.Document.Add(new CvfcDocument3
                {
                    CivilDocumentId = appearance.AppearanceId,
                    ImageId = appearance.AppearanceId,
                    DocumentTypeCd = "CSR",
                    LastAppearanceDt = appearance.AppearanceDate,
                    FiledDt = appearance.AppearanceDate
                });
            }

            //Add in documentTypeDescription.

            var civilFileDetail = _mapper.Map<RedactedCivilFileDetailResponse>(civilFileDetailResponse);
            return Ok(civilFileDetail);
        }

        /// <summary>
        /// Gets appearances for a given civil file id.
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="future">Y to show future, N to hide future</param>
        /// <param name="history">Y to show history, N to hide history</param>
        /// <returns>CivilFileAppearancesResponse</returns>
        [HttpGet]
        [Route("civil/{fileId}/appearances")]
        public async Task<ActionResult<CivilFileAppearancesResponse>> GetCivilAppearancesByFileId(string fileId, FutureYN2? future, HistoryYN2? history)
        {
            var criminalFileIdAppearances = await _fsClient.FilesCivilFileIdAppearancesAsync(_requestAgencyIdentifierId, _requestPartId, future, history, fileId);
            return Ok(criminalFileIdAppearances);
        }

        /// <summary>
        /// Gets court summary report for a given appearance id.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("civil/court-summary-report/{appearanceId}")]
        public async Task<ActionResult<CivilFileAppearancesResponse>> GetCivilCourtSummaryReport(string appearanceId)
        {
            throw new NotImplementedException("Working on the JC interface for this. ");
        }

        /// <summary>
        /// Gets the civil file content.
        /// This should cover the 1st screenshot. This includes document description, which is important for forming the PDF file names. 
        /// </summary>
        /// <param name="agencyId"></param>
        /// <param name="roomCode"></param>
        /// <param name="proceeding">DateTime? converted to yyyy-MM-dd string</param>
        /// <param name="appearanceId"></param>
        /// <param name="physicalFileId"></param>
        /// <returns>CivilFileContent</returns>
        [HttpGet]
        [Route("civil/file-content")]
        public async Task<ActionResult<CivilFileContent>> GetCivilFileContent(string agencyId = "", string roomCode = "", DateTime? proceeding = null, string appearanceId = "", string physicalFileId = "")
        {
            var proceedingDateString = proceeding.HasValue ? proceeding.Value.ToString("yyyy-MM-dd") : "";
            var civilFileContent = await _fsClient.FilesCivilFilecontentAsync(agencyId, roomCode, proceedingDateString,
                appearanceId, physicalFileId);
            return Ok(civilFileContent);
        }
        #endregion

        #region Criminal Only
        /// <summary>
        /// Provides facilities for performing a criminal file search.  
        /// This should cover the 5th screenshot. 
        /// </summary>
        /// <param name="fcq">FileCriminalQuery, composed of parameters for search. </param>
        /// <returns>FileSearchResponse</returns>
        [HttpPost]
        [Route("criminal/search")]
        public async Task<ActionResult<FileSearchResponse>> FilesCriminalSearchAsync(FilesCriminalQuery fcq)
        {
            fcq.FilePermissions =
                "[\"A\", \"Y\", \"T\", \"F\", \"C\", \"M\", \"L\", \"R\", \"B\", \"D\", \"E\", \"G\", \"H\", \"N\", \"O\", \"P\", \"S\", \"V\"]"; // for now, use all types - TODO: determine proper list of types?

            //CourtLevel = "S"  Supreme court data, CourtLevel = "P" - Province.
            var fileSearchResponse = await _fsClient.FilesCriminalAsync(_requestAgencyIdentifierId,
                _requestPartId, _requestApplicationCode, fcq.SearchMode, fcq.FileHomeAgencyId, fcq.FileNumberTxt,
                fcq.FilePrefixTxt, fcq.FilePermissions, fcq.FileSuffixNo, fcq.MdocRefTypeCode, fcq.CourtClass,
                fcq.CourtLevel, fcq.NameSearchTypeCd, fcq.LastName, fcq.OrgName, fcq.GivenName,
                fcq.Birth?.ToString("yyyy-MM-dd"), fcq.SearchByCrownPartId, fcq.SearchByCrownActiveOnly,
                fcq.SearchByCrownFileDesignation, fcq.MdocJustinNoSet, fcq.PhysicalFileIdSet);
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
            var criminalFileDetailResponse = await _fsClient.FilesCriminalFileIdAsync(_requestAgencyIdentifierId, _requestPartId, _requestApplicationCode, fileId);
            var redactedCriminalFileDetailResponse = _mapper.Map<RedactedCriminalFileDetailResponse>(criminalFileDetailResponse);
            return Ok(redactedCriminalFileDetailResponse);
        }

        /// <summary>
        /// Gets appearances for a given criminal file id.
        /// </summary>
        /// <param name="fileId">Target file id.</param>
        /// <param name="future">Y to show future, N to hide future.</param>
        /// <param name="history">Y to show history, N to hide history.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("criminal/{fileId}/appearances")]
        public async Task<ActionResult<CriminalFileAppearancesResponse>> GetCriminalAppearancesByFileId(string fileId, FutureYN? future = null, HistoryYN? history = null)
        {
            var criminalFileIdAppearances = await _fsClient.FilesCriminalFileIdAppearancesAsync(_requestAgencyIdentifierId, _requestPartId, future, history, fileId);
            return Ok(criminalFileIdAppearances);
        }

        /// <summary>
        /// Gets the criminal file content.
        /// This should cover some of the 3rd screenshot.
        /// </summary>
        /// <param name="agencyId"></param>
        /// <param name="roomCode"></param>
        /// <param name="proceeding">The proceeding date in the format YYYY-MM-dd</param>
        /// <param name="appearanceId"></param>
        /// <param name="justinNumber"></param>
        /// <returns>CriminalFileContent</returns>
        [HttpGet]
        [Route("criminal/file-content")]
        public async Task<ActionResult<CriminalFileContent>> GetCriminalFileContent(string agencyId, string roomCode, DateTime? proceeding, string appearanceId = null, string justinNumber = null)
        {
            var proceedingDateString = proceeding.HasValue ? proceeding.Value.ToString("yyyy-MM-dd") : "";
            var criminalFileContent = await _fsClient.FilesCriminalFilecontentAsync(agencyId, roomCode,
                proceedingDateString, appearanceId, justinNumber);
            return Ok(criminalFileContent);
        }
        #endregion

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
            var proceedingDateString = proceeding.HasValue ? proceeding.Value.ToString("yyyy-MM-dd") : "";
            var courtList = await _fsClient.FilesCourtlistAsync(agencyId, roomCode, proceedingDateString, divisionCode,
                fileNumber);
            return Ok(courtList);
        }

        /// <summary>
        /// Gets a document.
        /// Should cover 2nd and 4th screenshots? 
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="isCriminal">True if Criminal, False if Civil</param>
        /// <returns>DocumentResponse</returns>
        [HttpGet]
        [Route("document/{documentId}/{fileName?}")]
        public async Task<ActionResult<DocumentResponse>> GetDocument(string documentId, bool isCriminal = false)
        {
            var documentResponse = await _fsClient.FilesDocumentAsync(documentId, isCriminal ? "R" : "I");
            return Ok(documentResponse);
        }

        /// <summary>
        /// Gets records of proceedings.
        /// </summary>
        /// <param name="partId">The participant id associated to the Record Of Proceedings.</param>
        /// <param name="profSequenceNumber"></param>
        /// <param name="courtLevelCode">The associated court level code.</param>
        /// <param name="courtClassCode">The associated court class code.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("record-of-proceedings")]
        public async Task<ActionResult<CourtList>> GetRecordsOfProceeding(string partId, string profSequenceNumber, CourtLevelCd courtLevelCode, CourtClassCd courtClassCode)
        {
            var recordsOfProceeding = await _fsClient.FilesRecordOfProceedingsAsync(partId, profSequenceNumber, courtLevelCode, courtClassCode);
            return Ok(recordsOfProceeding);
        }




        #endregion

        #region Helpers
        /// <summary>
        /// This is used to set the baseUrl, and add in contract resolvers that allow fields to be null.
        /// </summary>
        private void SetupFileServicesClient()
        {
            _fsClient.JsonSerializerSettings.ContractResolver = new SafeContractResolver();
            _fsClient.BaseUrl = _configuration.GetValue<string>("FileServicesClient:Url") ?? throw new ConfigurationException($"Configuration 'FileServicesClient:Url' is invalid or missing.");
            _requestApplicationCode = _configuration.GetValue<string>("Request:ApplicationCd") ?? throw new ConfigurationException($"Configuration 'Request:ApplicationCd' is invalid or missing.");
            _requestAgencyIdentifierId = _configuration.GetValue<string>("Request:AgencyIdentifierId") ?? throw new ConfigurationException($"Configuration 'Request:AgencyIdentifierId' is invalid or missing.");
            _requestPartId = _configuration.GetValue<string>("Request:PartId") ?? throw new ConfigurationException($"Configuration 'Request:PartId' is invalid or missing.");
        }
        #endregion
    }
}