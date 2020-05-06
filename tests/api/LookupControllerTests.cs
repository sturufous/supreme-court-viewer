using JCCommon.Clients.LookupServices;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Scv.Api.Controllers;
using tests.api.Helpers;
using Xunit;

namespace tests.api
{
    /// <summary>
    /// These tests, ensure Api.LookupControllers and JC-Client-Interface.LookupServiceClient work correctly. 
    /// Credit to DARS for most of these tests. 
    /// </summary>
    public class LookupControllerTests
    {
        #region Variables
        private readonly LookupController _controller;
        #endregion


        #region Constructor
        public LookupControllerTests()
        {
            var preTest = new ApiControllerEnvironmentBuilder("LookupServicesClient:Username", "LookupServicesClient:Password", typeof(FilesController));
            _controller = new LookupController(preTest.Configuration, preTest.LogFactory.CreateLogger<LookupController>(), new LookupServiceClient(preTest.HttpClient), new Mapper());
        }
        #endregion


        #region Tests
        [Fact]
        public async void Document_Codes()
        {
            var actionResult = await _controller.GetDocumentCodes();

            var lookupCodes = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Contains(lookupCodes, fd => fd.CodeType == "DOCUMENT_TYPES");
        }

        #endregion
    }
}
