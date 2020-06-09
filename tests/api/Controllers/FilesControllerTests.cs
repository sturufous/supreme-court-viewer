using JCCommon.Clients.FileServices;
using JCCommon.Clients.LocationServices;
using JCCommon.Clients.LookupServices;
using JCCommon.Models;
using LazyCache;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Scv.Api.Controllers;
using Scv.Api.Services;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Scv.Api.Helpers.ContractResolver;
using Scv.Api.Models.Civil.Detail;
using Scv.Api.Models.Criminal.Detail;
using tests.api.Helpers;
using Xunit;

namespace tests.api.Controllers
{
    /// <summary>
    /// These tests, ensure Api.FilesController and JC-Client-Interface.FileServicesClient work correctly.
    /// Credit to DARS for most of these tests.
    /// Note: these tests are intended for the development environment. 
    /// </summary>
    public class FilesControllerTests
    {
        #region Variables

        private readonly FilesController _controller;

        #endregion Variables

        #region Constructor

        public FilesControllerTests()
        {
            var fileServices = new EnvironmentBuilder("FileServicesClient:Username", "FileServicesClient:Password", "FileServicesClient:Url");
            var lookupServices = new EnvironmentBuilder("LookupServicesClient:Username", "LookupServicesClient:Password", "LookupServicesClient:Url");
            var locationServices = new EnvironmentBuilder("LocationServicesClient:Username", "LocationServicesClient:Password", "LocationServicesClient:Url");
            var lookupServiceClient = new LookupServiceClient(lookupServices.HttpClient);
            var locationServiceClient = new LocationServicesClient(locationServices.HttpClient);
            var fileServicesClient = new FileServicesClient(fileServices.HttpClient);
            var lookupService = new LookupService(lookupServices.Configuration, lookupServiceClient, new CachingService());
            var locationService = new LocationService(locationServices.Configuration, locationServiceClient, new CachingService());
            var filesService = new FilesService(fileServices.Configuration, fileServicesClient, new Mapper(), lookupService, locationService, new CachingService());
            _controller = new FilesController(fileServices.Configuration, fileServices.LogFactory.CreateLogger<FilesController>(), filesService);
            SetupMocks();
        }

        #endregion Constructor

        #region Tests

        [Fact]
        public async void Criminal_File_Search_By_LastName()
        {
            var fcq = new FilesCriminalQuery()
            {
                SearchMode = SearchMode.PARTNAME,
                FileHomeAgencyId = "83.0001",
                LastName = "Sm"
            };
            var actionResult = await _controller.FilesCriminalSearchAsync(fcq);

            var fileSearchResponse = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Contains(fileSearchResponse.FileDetail, fd => fd.Participant.Any(p => p.FullNm.Contains("Smith")));
        }

        [Fact]
        public async void Civil_File_Search_By_LastName()
        {
            var fcq = new FilesCivilQuery()
            {
                SearchMode = SearchMode2.PARTNAME,
                FileHomeAgencyId = "83.0001",
                LastName = "bad",
                CourtLevel = CourtLevelCd3.P
            };
            var actionResult = await _controller.FilesCivilSearchAsync(fcq);

            var fileSearchResponse = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("1", fileSearchResponse.RecCount);
            Assert.Equal(1, fileSearchResponse.FileDetail.Count);
            Assert.Contains("2437", fileSearchResponse.FileDetail.First().PhysicalFileId);
            Assert.Contains("BADGUY, Borris", fileSearchResponse.FileDetail.First().Participant.Select(u => u.FullNm));
        }

        [Fact]
        public async void Criminal_File_Search_By_JustinNo()
        {
            var fcq = new FilesCriminalQuery()
            {
                SearchMode = SearchMode.JUSTINNO,
                FileHomeAgencyId = "83.00001",
                MdocJustinNoSet = "35674"
            };
            var actionResult = await _controller.FilesCriminalSearchAsync(fcq);

            var fileSearchResponse = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Contains(fileSearchResponse.FileDetail, fd => fd.MdocJustinNo == "35674");
        }

        [Fact]
        public async void Civil_File_Search_By_PhysicalFileId()
        {
            var fcq = new FilesCivilQuery()
            {
                SearchMode = SearchMode2.PHYSID,
                FileHomeAgencyId = "83.0001",
                PhysicalFileIdSet = "2506"
            };
            var actionResult = await _controller.FilesCivilSearchAsync(fcq);

            var fileSearchResponse = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("1", fileSearchResponse.RecCount);
            Assert.Equal(1, fileSearchResponse.FileDetail.Count);
            Assert.Equal("C-11011", fileSearchResponse.FileDetail.First().FileNumberTxt);
            Assert.Contains("BYSTANDER, Innocent", fileSearchResponse.FileDetail.First().Participant.Select(u => u.FullNm));
        }

        [Fact]
        public async void Criminal_File_Details_by_JustinNo()
        {
            var actionResult = await _controller.GetCriminalFileDetailByFileId("35674");

            var redactedCriminalFileDetailResponse = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("35674", redactedCriminalFileDetailResponse.JustinNo);
            Assert.True(redactedCriminalFileDetailResponse.Participant.Count > 0);
        }

        [Fact]
        public async void Civil_File_Details_by_PhysicalFileId()
        {
            var actionResult = await _controller.GetCivilFileDetailByFileId("40");

            var redactedCivilFileDetailResponse = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("40", redactedCivilFileDetailResponse.PhysicalFileId);
            Assert.Equal("P-241", redactedCivilFileDetailResponse.FileNumberTxt);
            Assert.Contains(redactedCivilFileDetailResponse.Party, f => f.LastNm == "Kings");
            Assert.Contains(redactedCivilFileDetailResponse.Party, f => f.LastNm == "Jones");
        }

        [Fact]
        public async void Criminal_Appearances_by_JustinNo()
        {
            var actionResult = await _controller.GetCriminalFileDetailByFileId("35674");

            var criminalFileAppearancesResponse = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Contains(criminalFileAppearancesResponse.Appearances.ApprDetail,
                f => f.LastNm == "Young" && f.GivenNm == "Johnny");
        }

        [Fact]
        public async void Civil_Appearances_by_PhysicalFileId()
        {
            var actionResult = await _controller.GetCivilFileDetailByFileId("2506");

            var civilFile = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("0", civilFile.Appearances.FutureRecCount);
            Assert.Equal("20", civilFile.Appearances.HistoryRecCount);
        }

        [Fact]
        public async void GetCourtlist()
        {
            var actionResult = await _controller.GetCourtList("4801", "101", DateTime.Parse("2016-04-04"), "CR", "4889-1");

            var criminalFileAppearancesResponse = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("4801", criminalFileAppearancesResponse.CourtLocationName);
            Assert.Equal("101", criminalFileAppearancesResponse.CourtRoomCode);
            Assert.Equal("2016-04-04", criminalFileAppearancesResponse.CourtProceedingsDate);
        }

        [Fact]
        public async void Criminal_File_Content()
        {
            var actionResult = await _controller.GetCriminalFileContent("4801", "101", DateTime.Parse("2016-04-04"), "44150.0734");

            var criminalFileContent = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("4801", criminalFileContent.CourtLocaCd);
            Assert.Equal("101", criminalFileContent.CourtRoomCd);
            Assert.Equal("2016-04-04", criminalFileContent.CourtProceedingDate);
        }

        [Fact]
        public async void Civil_File_Content_By_AgencyId_Room_Proceeding_Appearance()
        {
            var actionResult = await _controller.GetCivilFileContent("4801", "101", DateTime.Parse("2016-04-04"), "984");

            var civilFileContent = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("4801", civilFileContent.CourtLocaCd);
            Assert.Equal("101", civilFileContent.CourtRoomCd);
            Assert.Equal("2016-04-04", civilFileContent.CourtProceedingDate);
            Assert.Equal(1, civilFileContent.CivilFile.Count);
            Assert.Equal("2506", civilFileContent.CivilFile.First().PhysicalFileID);
        }

        [Fact]
        public async void Criminal_File_Search_By_FileNo_Provincial()
        {
            var fcq = new FilesCriminalQuery
            {
                SearchMode = SearchMode.FILENO,
                FileHomeAgencyId = "83.0001",
                FileNumberTxt = "98050101",
                CourtLevel = CourtLevelCd2.P
            };
            var actionResult = await _controller.FilesCriminalSearchAsync(fcq);

            var fileSearchResponse = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("2", fileSearchResponse.RecCount);
        }

        [Fact]
        public async void Criminal_File_Search_By_FileNo_Supreme()
        {
            var fcq = new FilesCriminalQuery
            {
                SearchMode = SearchMode.FILENO,
                FileHomeAgencyId = "83.0001",
                FileNumberTxt = "98050101",
                CourtLevel = CourtLevelCd2.S
            };
            var actionResult = await _controller.FilesCriminalSearchAsync(fcq);

            var fileSearchResponse = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("1", fileSearchResponse.RecCount);
        }

        [Fact]
        public async void Criminal_File_Search_By_FileNo_ProvincialAndSupreme()
        {
            var fcq = new FilesCriminalQuery
            {
                SearchMode = SearchMode.FILENO,
                FileHomeAgencyId = "83.0001",
                FileNumberTxt = "98050101",
            };
            var actionResult = await _controller.FilesCriminalSearchAsync(fcq);

            var fileSearchResponse = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("2", fileSearchResponse.RecCount);
        }

        [Fact]
        public async void Civil_File_Search_By_FileNo_Provincial()
        {
            var fcq = new FilesCivilQuery
            {
                SearchMode = SearchMode2.FILENO,
                FileHomeAgencyId = "83.0001",
                FileNumber = "11011",
                CourtLevel = CourtLevelCd3.P
            };
            var actionResult = await _controller.FilesCivilSearchAsync(fcq);

            var fileSearchResponse = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("1", fileSearchResponse.RecCount);
            Assert.Equal(1, fileSearchResponse.FileDetail.Count);
            Assert.Equal("2506", fileSearchResponse.FileDetail.First().PhysicalFileId);
            Assert.Contains("BYSTANDER, Innocent", fileSearchResponse.FileDetail.First().Participant.Select(u => u.FullNm));
        }

        [Fact]
        public async void Civil_Court_Summary_Report()
        {
            var actionResult = await _controller.GetCivilCourtSummaryReport("984", "test123.pdf");

            var fileContentResult = actionResult as FileContentResult;
            Assert.NotNull(fileContentResult);
            Assert.True(fileContentResult.FileContents.Length > 5100);
        }

        [Fact]
        public async void Criminal_Record_Of_Proceeding()
        {
            var actionResult = await _controller.GetRecordsOfProceeding("12971.0026", "ropTest.pdf", "24", CourtLevelCd.P, CourtClassCd.A);

            var fileContentResult = actionResult as FileContentResult;
            Assert.NotNull(fileContentResult);
            Assert.True(fileContentResult.FileContents.Length > 79000);
        }

        [Fact]
        public async void Civil_File_Content_By_FileId()
        {
            var actionResult = await _controller.GetCivilFileContent(physicalFileId: "2506");

            var civilFileContent = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Null(civilFileContent.CourtLocaCd);
            Assert.Null(civilFileContent.CourtRoomCd);
            Assert.Equal("", civilFileContent.CourtProceedingDate);
            Assert.Equal(1, civilFileContent.CivilFile.Count);
            Assert.Equal("2506", civilFileContent.CivilFile.First().PhysicalFileID);
        }

        [Fact]
        public async void Document_Civil()
        {
            var actionResult = await _controller.GetDocument("10010", "test.pdf");

            var fileContentResult = actionResult as FileContentResult;
            Assert.NotNull(fileContentResult);
            Assert.Equal(14625, fileContentResult.FileContents.Length);
        }

        [Fact]
        public async void Document_Criminal()
        {
            var actionResult = await _controller.GetDocument(documentId: "40", "test.pdf", true);

            var fileContentResult = actionResult as FileContentResult;
            Assert.NotNull(fileContentResult);
            Assert.Equal(782434, fileContentResult.FileContents.Length);
        }

        [Fact]
        public async void Criminal_File_Content_By_JustinNumber()
        {
            var actionResult = await _controller.GetCriminalFileContent(justinNumber: "3179.0000");

            var criminalFileContent = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("", criminalFileContent.CourtLocaCd);
            Assert.Equal("", criminalFileContent.CourtRoomCd);
            Assert.Equal("", criminalFileContent.CourtProceedingDate);
            Assert.Equal(1, criminalFileContent.AccusedFile.Count);
            Assert.Equal("3179", criminalFileContent.AccusedFile.First().MdocJustinNo);
        }

        #endregion Tests

        [Fact]
        public async void Criminal_File_Detail_Document_By_JustinNumber()
        {
            var actionResult = await _controller.GetCriminalFileDetailByFileId(fileId: "35840");

            var criminalFileDocuments = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal(4, criminalFileDocuments.Participant.First().Document.Count);
            Assert.Contains(criminalFileDocuments.Participant.First().Document,
                doc => doc.DocmFormDsc == "Summons Criminal Code (With a very long name so I can test Cannabis names)");
            Assert.Contains(criminalFileDocuments.Participant.First().Document, doc => doc.PartId == "61145.0002");
        }

        [Fact]
        public async void Civil_Appearance_Details()
        {
            //Has party data.
            var actionResult = await _controller.GetCivilAppearanceDetails("2506", "11034");

            var civilAppearanceDetail = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal(3, civilAppearanceDetail.Party.Count);
            Assert.Contains(civilAppearanceDetail.Party, p => p.LastNm == "BYSTANDER");

            //Has appearanceMethod data.
            actionResult = await _controller.GetCivilAppearanceDetails("3499", "13410");

            civilAppearanceDetail = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal(1, civilAppearanceDetail.AppearanceMethod.Count);
            Assert.Equal("IP", civilAppearanceDetail.AppearanceMethod.First().AppearanceMethodCd);
        }

        [Fact]
        public async void Criminal_Appearance_Details()
        {
            var actionResult = await _controller.GetCriminalAppearanceDetails("2934", "36548.0734", "19498.0042");

            var criminalAppearanceDetail = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal(1, criminalAppearanceDetail.Charges.Count);
            Assert.Equal("2934", criminalAppearanceDetail.JustinNo);
            Assert.Contains(criminalAppearanceDetail.Charges, p => p.AppearanceReasonDsc == "First Appearance");
            Assert.Contains(criminalAppearanceDetail.Charges, p => p.StatuteDsc == "offer bribe to justice/pol comm/peac off");
            Assert.Contains(criminalAppearanceDetail.Charges, p => p.StatuteSectionDsc == "CCC - 120(b)");
            Assert.Equal(1, criminalAppearanceDetail.AppearanceMethods.Count);
            Assert.Equal("TC", criminalAppearanceDetail.AppearanceMethods.First().AppearanceMethodCd);
        }

        [Fact]
        public async void Criminal_Appearance_Details_Accused_Prosecutor_Adjudicator()
        {
            var actionResult = await _controller.GetCriminalAppearanceDetails("2800", "34595.0734", "13816.0026");

            var criminalAppearanceDetail = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);

            Assert.Equal("2800", criminalAppearanceDetail.JustinNo);
            Assert.Contains(criminalAppearanceDetail.Charges, p => p.AppearanceReasonDsc == "First Appearance");
            Assert.Equal("JONES BARB", criminalAppearanceDetail.JustinCounsel.FullName);
            Assert.Equal("Woody Allan", criminalAppearanceDetail.Accused.FullName);
            Assert.Equal("Telephone Conference", criminalAppearanceDetail.Accused.AttendanceMethodDesc);
            Assert.Equal("Willie Smith", criminalAppearanceDetail.Prosecutor.FullName);
            Assert.Equal("26139.0045", criminalAppearanceDetail.Prosecutor.PartId);
            Assert.Equal("R Butler Mon Ami", criminalAppearanceDetail.Adjudicator.FullName);
        }

        [Fact]
        public async void Criminal_Appearance_Details_No_Prosecutor_Adjudicator()
        {
            var actionResult = await _controller.GetCriminalAppearanceDetails("2934", "36548.0734", "19498.0042");

            var criminalAppearanceDetail = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);

            Assert.Equal("2934", criminalAppearanceDetail.JustinNo);
            Assert.Equal("Telephone Conference", criminalAppearanceDetail.Accused.AttendanceMethodDesc);
            Assert.Equal("TC", criminalAppearanceDetail.Accused.AttendanceMethodCd);
            Assert.Equal("Telephone Conference", criminalAppearanceDetail.AppearanceMethods.First().AppearanceMethodDesc);
            Assert.Equal("TC", criminalAppearanceDetail.AppearanceMethods.First().AppearanceMethodCd);
            Assert.Equal("79221.0734", criminalAppearanceDetail.JustinCounsel.CounselPartId);
            Assert.Null(criminalAppearanceDetail.Prosecutor);
            Assert.Null(criminalAppearanceDetail.Adjudicator);
        }


        [Fact]
        public async void Criminal_Appearance_Details_Prosecutor_Adjudicator_Accused()
        {
            var actionResult = await _controller.GetCriminalAppearanceDetails("1009", "1169.0026", "14188.0026");

            var criminalAppearanceDetail = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);

            Assert.Equal("1009", criminalAppearanceDetail.JustinNo);
            Assert.Equal("Stephen Frank Lewis", criminalAppearanceDetail.Accused.FullName);
            Assert.Equal("P", criminalAppearanceDetail.Accused.PartyAppearanceMethod);
            Assert.Equal("Present", criminalAppearanceDetail.Accused.PartyAppearanceMethodDesc);
            Assert.Equal("Michael Jordan", criminalAppearanceDetail.Adjudicator.FullName);
            Assert.Equal("14007.0026", criminalAppearanceDetail.Adjudicator.PartId);
            Assert.Equal("Brad Bow Baggins Stez", criminalAppearanceDetail.Prosecutor.FullName);
            Assert.Equal("19.0001", criminalAppearanceDetail.Prosecutor.PartId);
        }


        [Fact]
        public async void Criminal_Appearance_Details_AttendanceMethod_PartyAppearanceMethod_AppearanceMethod()
        {
            var actionResult = await _controller.GetCriminalAppearanceDetails("3058", "30503.0734", "19621.0042");

            var criminalAppearanceDetail = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("VC", criminalAppearanceDetail.JustinCounsel.AppearanceMethodCd);
            Assert.Equal("VC", criminalAppearanceDetail.JustinCounsel.AttendanceMethodCd);
            Assert.Equal("CV", criminalAppearanceDetail.JustinCounsel.PartyAppearanceMethod);
            Assert.Equal("TC", criminalAppearanceDetail.Prosecutor.AppearanceMethodCd);
            Assert.Equal("TC", criminalAppearanceDetail.Prosecutor.AttendanceMethodCd);
            Assert.Equal("T", criminalAppearanceDetail.Prosecutor.PartyAppearanceMethod);
            Assert.Equal("VC", criminalAppearanceDetail.Accused.AppearanceMethodCd);
            Assert.Equal("VC", criminalAppearanceDetail.Accused.AttendanceMethodCd);
            Assert.Equal("PV", criminalAppearanceDetail.Accused.PartyAppearanceMethod);
        }

        [Fact(Skip= "Adhoc Test")]
        public async void Criminal_Appearance_Details_CacheTest()
        {
            //This fetches the FileDetail plus the appearances. So these should be cached after this call. 
            var actionResult = await _controller.GetCriminalFileDetailByFileId("2934");
            var fileDetail = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult);

            //Now call criminalAppearanceDetails. 
            var actionResult2 = await _controller.GetCriminalAppearanceDetails("2934", "36548.0734", "19498.0042");

            var criminalAppearanceDetail = HttpResponseTest.CheckForValidHttpResponseAndReturnValue(actionResult2);
            Assert.Equal(1, criminalAppearanceDetail.Charges.Count);
            Assert.Equal("2934", criminalAppearanceDetail.JustinNo);
            Assert.Contains(criminalAppearanceDetail.Charges, p => p.AppearanceReasonDsc == "First Appearance");
            Assert.Contains(criminalAppearanceDetail.Charges, p => p.StatuteDsc == "offer bribe to justice/pol comm/peac off");
            Assert.Contains(criminalAppearanceDetail.Charges, p => p.StatuteSectionDsc == "CCC - 120(b)");
            Assert.Equal(1, criminalAppearanceDetail.AppearanceMethods.Count);
            Assert.Equal("TC", criminalAppearanceDetail.AppearanceMethods.First().AppearanceMethodCd);
        }

        #region Helpers

        private void SetupMocks()
        {
            var headerDictionary = new HeaderDictionary();
            var response = new Mock<HttpResponse>();
            response.SetupGet(r => r.Headers).Returns(headerDictionary);

            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(a => a.Response).Returns(response.Object);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext.Object
            };
        }

        #endregion Helpers
    }
}