using System;
using System.Threading.Tasks;
using JCCommon.Clients.FileServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scv.Api.Helpers.ContractResolver;

namespace Scv.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        #region Variables
        private readonly IConfiguration _configuration;
        private readonly ILogger<FilesController> _logger;
        private readonly FileServicesClient _fsClient;
        #endregion

        #region Constructor
        public FilesController(IConfiguration configuration, ILogger<FilesController> logger, FileServicesClient fsClient)
        {
            _configuration = configuration;
            _logger = logger;
            _fsClient = fsClient;
            SetupFileServicesClient();
        }
        #endregion

        //TODO: Figure out what data depends on: DateTime proceedingDate, string locationCode, string room

        #region Actions
        /// <summary>
        /// This should cover some of the first screenshot.
        /// Should return a complex object with Documents, inside the documents there is a sequence number, date field, document type, issues.
        /// </summary>
        [HttpGet]
        [Route("CivilFileDetail/{fileNumber}")]

        public async Task<ActionResult<CivilFileDetailResponse>> GetCivilFileDetail(string fileNumber)
        {
            //fileId = 2506 works for this for some basic data. 
            return Ok(await _fsClient.FilesCivilFileIdAsync("", "", fileNumber));
        }

        /// <summary>
        /// This should cover some of the third screenshot.
        /// Should return a complex object with Documents, inside the documents there is a sequence number, date field, document type, issues.
        /// </summary>
        /// <param name="fileNumber"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CriminalFileDetail/{fileNumber}")]
        public async Task<ActionResult<CriminalFileDetailResponse>> GetCriminalFileDetail(string fileNumber)
        {
            return Ok(await _fsClient.FilesCivilFileIdAsync("", "", fileNumber));
        }
        #endregion

        #region Helpers
        /// <summary>
        /// This is used to set the baseUrl, and add in contract resolvers that allow fields to be null.
        /// </summary>
        private void SetupFileServicesClient()
        {
            _fsClient.BaseUrl = _configuration.GetValue<string>("FileServicesClient:Url");
            _fsClient.JsonSerializerSettings.ContractResolver = new SafeContractResolver();
        }
        #endregion
    }
}