using System;
using System.Linq;
using JCCommon.Clients.FileServices;
using JCCommon.Clients.LocationServices;
using JCCommon.Clients.LookupServices;
using LazyCache;
using MapsterMapper;
using Scv.Api.Controllers;
using Scv.Api.Services;
using tests.api.Helpers;
using Xunit;

namespace tests.api.Controllers
{
    public class CourtListControllerTests
    {
        #region Variables

        private readonly CourtListController _controller;

        #endregion Variables

        #region Constructor

        public CourtListControllerTests()
        {
            var fileServices = new EnvironmentBuilder("FileServicesClient:Username", "FileServicesClient:Password", "FileServicesClient:Url");
            var lookupServices = new EnvironmentBuilder("LookupServicesClient:Username", "LookupServicesClient:Password", "LookupServicesClient:Url");
            var locationServices = new EnvironmentBuilder("LocationServicesClient:Username", "LocationServicesClient:Password", "LocationServicesClient:Url");
            var lookupServiceClient = new LookupServiceClient(lookupServices.HttpClient);
            var locationServiceClient = new LocationServicesClient(locationServices.HttpClient);
            var fileServicesClient = new FileServicesClient(fileServices.HttpClient);
            var lookupService = new LookupService(lookupServices.Configuration, lookupServiceClient, new CachingService());
            var locationService = new LocationService(locationServices.Configuration, locationServiceClient, new CachingService());
            var courtListService = new CourtListService(fileServices.Configuration, fileServicesClient, new Mapper(), lookupService, locationService, new CachingService());
            _controller = new CourtListController(courtListService)
            {
                ControllerContext = HttpResponseTest.SetupMockControllerContext()
            };
        }

        #endregion Constructor

        [Fact]
        public async void GetCourtList()
        {
            var actionResult = await _controller.GetCourtList("4801", "101", DateTime.Parse("2016-04-04"), "CR", "4889-1");

            var courtListResponse = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("4801", courtListResponse.CourtLocationName);
            Assert.Equal("101", courtListResponse.CourtRoomCode);
            Assert.Equal("2016-04-04", courtListResponse.CourtProceedingsDate);
        }

        [Fact]
        public async void GetCourtList_Criminal_And_Civil_Files()
        {
            var actionResult = await _controller.GetCourtList("4801", "101", DateTime.Parse("2015-10-22"), null, "C-996");

            var courtListResponse = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("4801", courtListResponse.CourtLocationName);
            Assert.Equal("101", courtListResponse.CourtRoomCode);
            Assert.Equal("2015-10-22", courtListResponse.CourtProceedingsDate);
            Assert.Equal("Continuation of Trial or Hearing", courtListResponse.CriminalCourtList.First().AppearanceReasonDesc);
        }

        [Fact]
        public async void GetCourtList_Criminal_Crown()
        {
            var actionResult = await _controller.GetCourtList("4801", "101", DateTime.Parse("2019-11-15"), null, null);

            var courtListResponse = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("4801", courtListResponse.CourtLocationName);
            Assert.Equal("101", courtListResponse.CourtRoomCode);
            Assert.Equal("2019-11-15", courtListResponse.CourtProceedingsDate);
            Assert.Contains(courtListResponse.CriminalCourtList, f => f.Crown.Count > 0);
        }

        #region Helpers

        #endregion Helpers

    }
}
