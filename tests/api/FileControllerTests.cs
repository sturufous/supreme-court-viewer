using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using JCCommon.Clients.FileServices;
using JCCommon.Framework;
using JCCommon.Models;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scv.Api.Controllers;
using Scv.Api.Helpers.Exceptions;
using Xunit;

namespace tests.api
{
    /// <summary>
    /// These tests, ensure Api.FilesController and JC-Client-Interface.FileServicesClient work correctly. 
    /// Credit to DARS for most of these tests. 
    /// </summary>
    public class FilesControllerTests
    {
        #region Variables
        private readonly FilesController _controller;
        #endregion

        #region Constructor
        public FilesControllerTests()
        {
            //Load in secrets, this uses a separate secrets file for the tests project. 
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets<FilesControllerTests>();
            IConfiguration configuration = builder.Build();

            //Create HTTP client, usually done by Startup.cs - which handles the life cycle of HttpClient nicely. 
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(
                configuration.GetValue<string>("FileServicesClient:Username") ?? throw new ConfigurationException("FileServicesClient:Username was not found in secrets."),
                configuration.GetValue<string>("FileServicesClient:Password") ?? throw new ConfigurationException("FileServicesClient:Password was not found in secrets."));

            //Create logger.
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                    .AddConsole();
            });

            _controller = new FilesController(configuration, loggerFactory.CreateLogger<FilesController>(), new FileServicesClient(client), new Mapper()  );
        }
        #endregion

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

            var fileSearchResponse = CheckForValidHttpResponseAndReturnValue(actionResult);
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

            var fileSearchResponse = CheckForValidHttpResponseAndReturnValue(actionResult);
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

            var fileSearchResponse = CheckForValidHttpResponseAndReturnValue(actionResult);
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

            var fileSearchResponse = CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("1", fileSearchResponse.RecCount);
            Assert.Equal(1, fileSearchResponse.FileDetail.Count);
            Assert.Equal("C-11011", fileSearchResponse.FileDetail.First().FileNumberTxt);
            Assert.Contains("BYSTANDER, Innocent", fileSearchResponse.FileDetail.First().Participant.Select(u => u.FullNm));
        }

        [Fact]
        public async void Criminal_File_Details_by_JustinNo()
        {
            var actionResult = await _controller.GetCriminalFileDetailByFileId("35674");

            var redactedCriminalFileDetailResponse = CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("35674", redactedCriminalFileDetailResponse.JustinNo);
            Assert.True(redactedCriminalFileDetailResponse.Participant.Count > 0);
        }

        [Fact]
        public async void Civil_File_Details_by_PhysicalFileId()
        {
            var actionResult = await _controller.GetCivilFileDetailByFileId("40");

            var redactedCivilFileDetailResponse = CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("40", redactedCivilFileDetailResponse.PhysicalFileId);
            Assert.Equal("P-241", redactedCivilFileDetailResponse.FileNumberTxt);
            Assert.Contains(redactedCivilFileDetailResponse.Party, f => f.LastNm == "Kings");
            Assert.Contains(redactedCivilFileDetailResponse.Party, f => f.LastNm == "Jones");
        }

        [Fact]
        public async void Criminal_Appearances_by_JustinNo()
        {
            var actionResult = await _controller.GetCriminalAppearancesByFileId("35674");

            var criminalFileAppearancesResponse = CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Contains(criminalFileAppearancesResponse.ApprDetail,
                f => f.LastNm == "Young" && f.GivenNm == "Johnny");
        }

        [Fact]
        public async void Civil_Appearances_by_PhysicalFileId()
        {
            var actionResult = await _controller.GetCivilAppearancesByFileId("2506",FutureYN2.Y, HistoryYN2.Y);

            var civilFileAppearancesByFileId = CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("0", civilFileAppearancesByFileId.FutureRecCount);
            Assert.Equal("20", civilFileAppearancesByFileId.HistoryRecCount);
        }

        [Fact]
        public async void Criminal_Appearances_by_JustinNo_0()
        {
            var actionResult = await _controller.GetCriminalAppearancesByFileId("0");

            var criminalFileAppearancesResponse = CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("0", criminalFileAppearancesResponse.FutureRecCount);
            Assert.Equal("0", criminalFileAppearancesResponse.HistoryRecCount);
        }

        [Fact]
        public async void GetCourtlist()
        {
            var actionResult = await _controller.GetCourtList("4801", "101", DateTime.Parse("2016-04-04"), "CR", "4889-1");

            var criminalFileAppearancesResponse = CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("4801", criminalFileAppearancesResponse.CourtLocationName);
            Assert.Equal("101", criminalFileAppearancesResponse.CourtRoomCode);
            Assert.Equal("2016-04-04", criminalFileAppearancesResponse.CourtProceedingsDate);
        }

        [Fact]
        public async void CriminalFileContent()
        {
            var actionResult = await _controller.GetCriminalFileContent("4801", "101", DateTime.Parse("2016-04-04"), "44150.0734");

            var criminalFileContent = CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("4801",criminalFileContent.CourtLocaCd);
            Assert.Equal("101", criminalFileContent.CourtRoomCd);
            Assert.Equal("2016-04-04", criminalFileContent.CourtProceedingDate);
        }

        [Fact]
        public async void CivilFileContent()
        {
            var actionResult = await _controller.GetCivilFileContent("4801", "101", DateTime.Parse("2016-04-04"), "984");

            var civilFileContent = CheckForValidHttpResponseAndReturnValue(actionResult);
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

            var fileSearchResponse = CheckForValidHttpResponseAndReturnValue(actionResult);
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

            var fileSearchResponse = CheckForValidHttpResponseAndReturnValue(actionResult);
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

            var fileSearchResponse = CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("3", fileSearchResponse.RecCount);
        }

        [Fact]
        public async void Civil_File_Search_By_FileNo_Provincial()
        {
            var fcq = new FilesCivilQuery
            {
                SearchMode = SearchMode2.FILENO,
                FileHomeAgencyId = "83.0001",
                FileNumber =  "11011",
                CourtLevel =  CourtLevelCd3.P
            };
            var actionResult = await _controller.FilesCivilSearchAsync(fcq);

            var fileSearchResponse = CheckForValidHttpResponseAndReturnValue(actionResult);
            Assert.Equal("1", fileSearchResponse.RecCount);
            Assert.Equal(1, fileSearchResponse.FileDetail.Count);
            Assert.Equal("2506", fileSearchResponse.FileDetail.First().PhysicalFileId);
            Assert.Contains("BYSTANDER, Innocent", fileSearchResponse.FileDetail.First().Participant.Select(u => u.FullNm));
        }
        #endregion

        #region Helpers
        private T CheckForValidHttpResponseAndReturnValue<T>(ActionResult<T> actionResult)
        {
            Assert.NotNull(actionResult);
            Assert.NotNull(actionResult.Result);
            var okObjectResult = actionResult.Result as OkObjectResult;
            Assert.NotNull(okObjectResult);
            var result = (T) okObjectResult.Value;
            Assert.NotNull(result);
            return result;
        }
        #endregion
    }
}
