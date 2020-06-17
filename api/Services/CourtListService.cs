using JCCommon.Clients.FileServices;
using LazyCache;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Serialization;
using Scv.Api.Helpers;
using Scv.Api.Helpers.ContractResolver;
using Scv.Api.Helpers.Extensions;
using Scv.Api.Models.Civil.CourtList;
using Scv.Api.Models.Criminal.CourtList;
using Scv.Api.Models.Criminal.Detail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scv.Api.Services
{
    public class CourtListService
    {
        #region Variables

        private readonly FileServicesClient _filesClient;
        private readonly LookupService _lookupService;
        private readonly IAppCache _cache;
        private readonly IMapper _mapper;
        private readonly string _requestApplicationCode;
        private readonly string _requestAgencyIdentifierId;
        private readonly string _requestPartId;

        #endregion Variables

        #region Constructor

        public CourtListService(IConfiguration configuration, FileServicesClient filesClient, IMapper mapper, LookupService lookupService, LocationService locationService, IAppCache cache)
        {
            _filesClient = filesClient;
            _filesClient.JsonSerializerSettings.ContractResolver = new SafeContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
            _cache = cache;
            _cache.DefaultCachePolicy.DefaultCacheDurationSeconds = int.Parse(configuration.GetNonEmptyValue("Caching:FileExpiryMinutes")) * 60;
            _mapper = mapper;

            _lookupService = lookupService;
            _requestApplicationCode = configuration.GetNonEmptyValue("Request:ApplicationCd");
            _requestAgencyIdentifierId = configuration.GetNonEmptyValue("Request:AgencyIdentifierId");
            _requestPartId = configuration.GetNonEmptyValue("Request:PartId");
        }

        #endregion Constructor

        public async Task<Models.CourtList.CourtList> CourtListAsync(string agencyId, string roomCode, DateTime? proceeding, string divisionCode, string fileNumber)
        {
            var proceedingDateString = proceeding.HasValue ? proceeding.Value.ToString("yyyy-MM-dd") : "";
            async Task<CourtList> CourtList() => await _filesClient.FilesCourtlistAsync(agencyId, roomCode, proceedingDateString, divisionCode, fileNumber);
            var originalCourtList = await _cache.GetOrAddAsync($"CivilCourtList-{agencyId}-{roomCode}-{proceedingDateString}-{fileNumber}", CourtList);

            if (originalCourtList == null)
                return null;

            var courtList = _mapper.Map<Models.CourtList.CourtList>(originalCourtList);

            //Start file details tasks.
            var civilFileDetailTasks = CivilFileDetailTasks(courtList);
            var criminalFileDetailTasks = CriminalFileDetailTasks(courtList);

            //Start appearances tasks.
            var civilAppearanceTasks = CivilAppearancesTasks(proceeding, courtList);
            var criminalAppearanceTasks = CriminalAppearancesTasks(proceeding, courtList);

            //Await our asynchronous requests.
            var civilFileDetails = (await civilFileDetailTasks.WhenAll()).ToList();
            var criminalFileDetails = (await criminalFileDetailTasks.WhenAll()).ToList();
            var civilAppearances = (await civilAppearanceTasks.WhenAll()).ToList();
            var criminalAppearances = (await criminalAppearanceTasks.WhenAll()).ToList();

            //Join court list + file details + appearances.
            courtList.CivilCourtList = await PopulateCivilFiles(civilFileDetails, courtList.CivilCourtList, civilAppearances);
            courtList.CriminalCourtList = await PopulateCriminalFiles(criminalFileDetails, courtList.CriminalCourtList, criminalAppearances);

            return courtList;
        }

        #region Helpers

        private List<Task<CivilFileAppearancesResponse>> CivilAppearancesTasks(DateTime? proceeding, Models.CourtList.CourtList courtList)
        {
            var targetDateInPast = DateTime.Now > proceeding;
            var lookForPastAppearances = targetDateInPast ? HistoryYN2.Y : HistoryYN2.N;
            var lookForFutureAppearances = targetDateInPast ? FutureYN2.N : FutureYN2.Y;

            var appearanceTasks = new List<Task<CivilFileAppearancesResponse>>();
            foreach (var fileId in courtList.CivilCourtList.Select(ccl => ccl.PhysicalFile.PhysicalFileID))
            {
                async Task<CivilFileAppearancesResponse> Appearances() => await _filesClient.FilesCivilFileIdAppearancesAsync(_requestAgencyIdentifierId, _requestPartId, lookForFutureAppearances,
                    lookForPastAppearances, fileId);
                appearanceTasks.Add(_cache.GetOrAddAsync($"CivilAppearances-{fileId}", Appearances));
            }

            return appearanceTasks;
        }

        private List<Task<CriminalFileAppearancesResponse>> CriminalAppearancesTasks(DateTime? proceeding, Models.CourtList.CourtList courtList)
        {
            var targetDateInPast = DateTime.Now > proceeding;
            var lookForPastAppearances = targetDateInPast ? HistoryYN.Y : HistoryYN.N;
            var lookForFutureAppearances = targetDateInPast ? FutureYN.N : FutureYN.Y;

            var appearanceTasks = new List<Task<CriminalFileAppearancesResponse>>();
            foreach (var fileId in courtList.CriminalCourtList.Select(ccl => ccl.FileInformation.MdocJustinNo))
            {
                async Task<CriminalFileAppearancesResponse> Appearances() => await _filesClient.FilesCriminalFileIdAppearancesAsync(
                    _requestAgencyIdentifierId, _requestPartId, lookForFutureAppearances,
                    lookForPastAppearances, fileId);
                appearanceTasks.Add(_cache.GetOrAddAsync($"CriminalAppearances-{fileId}", Appearances));
            }

            return appearanceTasks;
        }

        private List<Task<CriminalFileDetailResponse>> CriminalFileDetailTasks(Models.CourtList.CourtList courtList)
        {
            var fileDetailTasks = new List<Task<CriminalFileDetailResponse>>();
            foreach (var fileId in courtList.CriminalCourtList.Select(ccl => ccl.FileInformation.MdocJustinNo))
            {
                async Task<CriminalFileDetailResponse> FileDetails() =>
                    await _filesClient.FilesCriminalFileIdAsync(_requestAgencyIdentifierId, _requestPartId, _requestApplicationCode,
                        fileId);
                fileDetailTasks.Add(_cache.GetOrAddAsync($"CriminalFileDetail-{fileId}", FileDetails));
            }

            return fileDetailTasks;
        }

        private List<Task<CivilFileDetailResponse>> CivilFileDetailTasks(Models.CourtList.CourtList courtList)
        {
            var fileDetailTasks = new List<Task<CivilFileDetailResponse>>();
            foreach (var fileId in courtList.CivilCourtList.Select(ccl => ccl.PhysicalFile.PhysicalFileID))
            {
                async Task<CivilFileDetailResponse> FileDetails() => await _filesClient.FilesCivilFileIdAsync(_requestAgencyIdentifierId, _requestPartId, fileId);
                fileDetailTasks.Add(_cache.GetOrAddAsync($"CivilFileDetail-{fileId}", FileDetails));
            }

            return fileDetailTasks;
        }

        private async Task<ICollection<CivilCourtList>> PopulateCivilFiles(List<CivilFileDetailResponse> civilFileDetails, ICollection<CivilCourtList> courtList, ICollection<CivilFileAppearancesResponse> civilAppearances)
        {
            foreach (var courtListFile in courtList)
            {
                var fileDetail = civilFileDetails.FirstOrDefault(x => x.PhysicalFileId == courtListFile.PhysicalFile.PhysicalFileID);
                courtListFile.ActivityClassCd = await _lookupService.GetActivityClassCd(fileDetail?.CourtClassCd.ToString());

                foreach (var hearingRestriction in courtListFile.HearingRestriction)
                {
                    hearingRestriction.HearingRestrictionTypeDesc = await _lookupService.GetHearingRestrictionDescription(hearingRestriction.HearingRestrictiontype);
                }

                var targetAppearanceResponse = civilAppearances.FirstOrDefault(ca => ca.ApprDetail.Any(ad => ad.AppearanceId == courtListFile.AppearanceId));
                var targetAppearance = targetAppearanceResponse?.ApprDetail.FirstOrDefault(ad => ad.AppearanceId == courtListFile.AppearanceId);
                if (targetAppearance != null)
                {
                    courtListFile.AppearanceReasonCd = targetAppearance?.AppearanceReasonCd;
                    courtListFile.AppearanceReasonDesc =
                        await _lookupService.GetCivilAppearanceReasonsDescription(targetAppearance?.AppearanceReasonCd);
                    courtListFile.OutOfTownJudge = targetAppearance.OutOfTownJudgeTxt;
                    courtListFile.SecurityRestriction = targetAppearance.SecurityRestrictionTxt;
                    courtListFile.SupplementalEquipment = targetAppearance.SupplementalEquipmentTxt;
                    courtListFile.JudgeInitials = targetAppearance.JudgeInitials;
                    courtListFile.EstimatedTimeHour = targetAppearance.EstimatedTimeHour?.ReturnNullIfEmpty();
                    courtListFile.EstimatedTimeMin = targetAppearance.EstimatedTimeMin?.ReturnNullIfEmpty();
                }
            }

            return courtList;
        }

        private async Task<ICollection<CriminalCourtList>> PopulateCriminalFiles(List<CriminalFileDetailResponse> criminalFileDetails, ICollection<CriminalCourtList> courtList, ICollection<CriminalFileAppearancesResponse> criminalAppearances)
        {
            foreach (var courtListFile in courtList)
            {
                var fileDetail = criminalFileDetails.FirstOrDefault(x => x.JustinNo == courtListFile.FileInformation.MdocJustinNo);
                courtListFile.ActivityClassCd = await _lookupService.GetActivityClassCd(fileDetail?.CourtClassCd.ToString());
                courtListFile.Crown = PopulateCrown(fileDetail);
            
                foreach (var hearingRestriction in courtListFile.HearingRestriction)
                {
                    hearingRestriction.HearingRestrictionTypeDesc = await _lookupService.GetHearingRestrictionDescription(hearingRestriction.HearingRestrictiontype);
                }

                var targetAppearanceResponse = criminalAppearances.FirstOrDefault(ca => ca.ApprDetail.Any(ad => ad.AppearanceId == courtListFile.CriminalAppearanceID));
                var targetAppearance = targetAppearanceResponse?.ApprDetail.FirstOrDefault(ad => ad.AppearanceId == courtListFile.CriminalAppearanceID);
                if (targetAppearance != null)
                {
                    courtListFile.AppearanceReasonCd = targetAppearance?.AppearanceReasonCd;
                    courtListFile.AppearanceReasonDesc =
                        await _lookupService.GetCivilAppearanceReasonsDescription(targetAppearance?.AppearanceReasonCd);
                    courtListFile.OutOfTownJudge = targetAppearance.OutOfTownJudgeTxt;
                    courtListFile.SecurityRestriction = targetAppearance.SecurityRestrictionTxt;
                    courtListFile.SupplementalEquipment = targetAppearance.SupplementalEquipmentTxt;
                    courtListFile.JudgeInitials = targetAppearance.JudgeInitials;
                    courtListFile.EstimatedTimeHour = targetAppearance.EstimatedTimeHour?.ReturnNullIfEmpty();
                    courtListFile.EstimatedTimeMin = targetAppearance.EstimatedTimeMin?.ReturnNullIfEmpty();
                }
            }

            return courtList;
        }

        private ICollection<CrownWitness> PopulateCrown(CriminalFileDetailResponse fileDetail)
        {
            if (fileDetail == null)
                return null;

            var crown =
                _mapper.Map<ICollection<CrownWitness>>(fileDetail.Witness.Where(w => w.RoleTypeCd == CriminalWitnessRoleTypeCd.CRN)
                    .ToList());
            foreach (var crownWitness in crown)
            {
                crownWitness.Assigned = crownWitness.IsAssigned(fileDetail.AssignedPartNm);
            }

            return crown;
        }

        #endregion Helpers
    }
}