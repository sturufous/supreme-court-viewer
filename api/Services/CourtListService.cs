using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        #endregion

        public async Task<Api.Models.CourtList.CourtList> CourtListAsync(string agencyId, string roomCode, DateTime? proceeding, string divisionCode, string fileNumber)
        {
            var proceedingDateString = proceeding.HasValue ? proceeding.Value.ToString("yyyy-MM-dd") : "";
            var originalCourtList = await _filesClient.FilesCourtlistAsync(agencyId, roomCode, proceedingDateString, divisionCode,
                fileNumber);

            if (originalCourtList == null)
                return null;

            var courtList = _mapper.Map<Api.Models.CourtList.CourtList>(originalCourtList);
            //Populate file details. 
            var civilFileTasks = courtList.CivilCourtList.Select(civilFile => _filesClient.FilesCivilFileIdAsync(_requestAgencyIdentifierId, _requestPartId, civilFile.PhysicalFile.PhysicalFileID)).ToList();
            var criminalFileTasks = courtList.CriminalCourtList.Select(criminalFile => _filesClient.FilesCriminalFileIdAsync(_requestAgencyIdentifierId, _requestPartId, _requestApplicationCode,
                    criminalFile.FileInformation.MdocJustinNo)).ToList();

            var targetDateInPast = DateTime.Now > proceeding;
            var lookForPastAppearancesCivil = targetDateInPast ? HistoryYN2.Y : HistoryYN2.N;
            var lookForFutureAppearancesCivil = targetDateInPast ? FutureYN2.N : FutureYN2.Y;

            var lookForPastAppearancesCriminal = targetDateInPast ? HistoryYN.Y : HistoryYN.N;
            var lookForFutureAppearancesCriminal = targetDateInPast ? FutureYN.N : FutureYN.Y;

            //Populate appearances. 
            var civilAppearanceTasks = courtList.CivilCourtList.Select(ccl => ccl.PhysicalFile.PhysicalFileID).Select(fileId =>
                _filesClient.FilesCivilFileIdAppearancesAsync(_requestAgencyIdentifierId, _requestPartId, lookForFutureAppearancesCivil, lookForPastAppearancesCivil, fileId));
            var criminalAppearanceTasks = courtList.CriminalCourtList.Select(ccl => ccl.FileInformation.MdocJustinNo).Select(fileId =>
                _filesClient.FilesCriminalFileIdAppearancesAsync(_requestAgencyIdentifierId, _requestPartId, lookForFutureAppearancesCriminal, lookForPastAppearancesCriminal, fileId));

            //Await our asynchronous requests. 
            var civilFileDetails = (await civilFileTasks.WhenAll()).ToList();
            var criminalFileDetails = (await criminalFileTasks.WhenAll()).ToList();
            var civilAppearances = (await civilAppearanceTasks.WhenAll()).ToList();
            var criminalAppearances = (await criminalAppearanceTasks.WhenAll()).ToList();

            //Join court list + file details + appearances. 
            courtList.CivilCourtList = await PopulateCivilFiles(civilFileDetails, courtList.CivilCourtList, civilAppearances);
            courtList.CriminalCourtList = await PopulateCriminalFiles(criminalFileDetails, courtList.CriminalCourtList, criminalAppearances);
    
            return courtList;
        }

        #region Helpers

        private async Task<ICollection<CivilCourtList>> PopulateCivilFiles(List<CivilFileDetailResponse> civilFileDetails, ICollection<CivilCourtList> courtList, ICollection<CivilFileAppearancesResponse> civilAppearances)
        {
            foreach (var courtListFile in courtList)
            {
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
                }
            }

            return courtList;
        }

        private async Task<ICollection<CriminalCourtList>> PopulateCriminalFiles(List<CriminalFileDetailResponse> criminalFileDetails, ICollection<CriminalCourtList> courtList, ICollection<CriminalFileAppearancesResponse> criminalAppearances)
        {
            foreach (var courtListFile in courtList)
            {
                var fileDetail = criminalFileDetails.FirstOrDefault(x => x.JustinNo == courtListFile.FileInformation.MdocJustinNo);
                if (fileDetail != null)
                {
                    courtListFile.Crown =
                        _mapper.Map<ICollection<CrownWitness>>(fileDetail.Witness.Where(w => w.RoleTypeCd == CriminalWitnessRoleTypeCd.CRN)
                            .ToList());
                }


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
                }
            }

            return courtList;
        }

        #endregion

    }
}
