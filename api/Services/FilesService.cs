using JCCommon.Clients.FileServices;
using JCCommon.Models;
using LazyCache;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Serialization;
using Scv.Api.Helpers;
using Scv.Api.Helpers.ContractResolver;
using Scv.Api.Models.Civil.AppearanceDetail;
using Scv.Api.Models.Civil.Appearances;
using Scv.Api.Models.Civil.Detail;
using Scv.Api.Models.Criminal.AppearanceDetail;
using Scv.Api.Models.Criminal.Appearances;
using Scv.Api.Models.Criminal.Detail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scv.Api.Helpers.Extensions;
using CivilAppearanceDetail = Scv.Api.Models.Civil.AppearanceDetail.CivilAppearanceDetail;
using CriminalAppearanceDetail = Scv.Api.Models.Criminal.AppearanceDetail.CriminalAppearanceDetail;
using CriminalAppearanceMethod = Scv.Api.Models.Criminal.AppearanceDetail.CriminalAppearanceMethod;
using CriminalParticipant = Scv.Api.Models.Criminal.Detail.CriminalParticipant;
using CriminalWitness = Scv.Api.Models.Criminal.Detail.CriminalWitness;

namespace Scv.Api.Services
{
    /// <summary>
    /// This is meant to wrap our FileServicesClient. That way we can easily extend the information provided to us by the FileServicesClient.
    /// </summary>
    public class FilesService
    {
        #region Variables

        private readonly IAppCache _cache;
        private readonly FileServicesClient _filesClient;
        private readonly IMapper _mapper;
        private readonly LookupService _lookupService;
        private readonly LocationService _locationService;
        private readonly string _requestApplicationCode;
        private readonly string _requestAgencyIdentifierId;
        private readonly string _requestPartId;

        #endregion Variables

        #region Constructor

        public FilesService(IConfiguration configuration, FileServicesClient filesClient, IMapper mapper, LookupService lookupService, LocationService locationService, IAppCache cache)
        {
            _filesClient = filesClient;
            _filesClient.JsonSerializerSettings.ContractResolver = new SafeContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
            _lookupService = lookupService;
            _locationService = locationService;
            _mapper = mapper;
            _requestApplicationCode = configuration.GetNonEmptyValue("Request:ApplicationCd");
            _requestAgencyIdentifierId = configuration.GetNonEmptyValue("Request:AgencyIdentifierId");
            _requestPartId = configuration.GetNonEmptyValue("Request:PartId");
            _cache = cache;
            _cache.DefaultCachePolicy.DefaultCacheDurationSeconds = int.Parse(configuration.GetNonEmptyValue("Caching:FileExpiryMinutes")) * 60;
        }

        #endregion Constructor

        #region Methods

        #region Civil Only

        public async Task<FileSearchResponse> CivilSearchAsync(FilesCivilQuery fcq)
        {
            fcq.FilePermissions =
                "[\"A\", \"Y\", \"T\", \"F\", \"C\", \"M\", \"L\", \"R\", \"B\", \"D\", \"E\", \"G\", \"H\", \"N\", \"O\", \"P\", \"S\", \"V\"]"; // for now, use all types - TODO: determine proper list of types?
            return await _filesClient.FilesCivilAsync(_requestAgencyIdentifierId, _requestPartId,
                _requestApplicationCode, fcq.SearchMode, fcq.FileHomeAgencyId, fcq.FileNumber, fcq.FilePrefix,
                fcq.FilePermissions, fcq.FileSuffixNumber, fcq.MDocReferenceTypeCode, fcq.CourtClass, fcq.CourtLevel,
                fcq.NameSearchType, fcq.LastName, fcq.OrgName, fcq.GivenName, fcq.Birth?.ToString("yyyy-MM-dd"),
                fcq.SearchByCrownPartId, fcq.SearchByCrownActiveOnly, fcq.SearchByCrownFileDesignation,
                fcq.MdocJustinNumberSet, fcq.PhysicalFileIdSet);
        }

        public async Task<RedactedCivilFileDetailResponse> CivilFileIdAsync(string fileId)
        {
            async Task<CivilFileDetailResponse> FileDetails() => await _filesClient.FilesCivilFileIdAsync(_requestAgencyIdentifierId, _requestPartId, fileId);
            async Task<CivilFileContent> FileContent() => await _filesClient.FilesCivilFilecontentAsync(null,null,null,null,fileId);
            async Task<CivilFileAppearancesResponse> Appearances() => await PopulateCivilDetailAppearancesAsync(FutureYN2.Y, HistoryYN2.Y, fileId);

            var fileDetailTask = _cache.GetOrAddAsync($"CivilFileDetail-{fileId}", FileDetails);
            var fileContentTask = _cache.GetOrAddAsync($"CivilFileContent-{fileId}", FileContent);
            var appearancesTask = _cache.GetOrAddAsync($"CivilAppearancesFull-{fileId}", Appearances);

            var fileDetail = await fileDetailTask;
            var appearances = await appearancesTask;
            var fileContent = await fileContentTask;

            if (fileDetail.PhysicalFileId == null)
                return null;

            var detail = _mapper.Map<RedactedCivilFileDetailResponse>(fileDetail);
            foreach (var document in PopulateCivilDetailCsrsDocuments(fileDetail.Appearance)) 
                detail.Document.Add(document);

            detail = await PopulateBaseCivilDetail(detail);
            detail.Appearances = appearances;
            detail.FileCommentText = fileContent.CivilFile.First(cf => cf.PhysicalFileID == fileId).FileCommentText;
            detail.Party = await PopulateCivilDetailParties(detail.Party);
            detail.Document = await PopulateCivilDetailDocuments(detail.Document);
            detail.HearingRestriction = await PopulateCivilDetailHearingRestrictions(fileDetail.HearingRestriction);
            return detail;
        }

        public async Task<CivilAppearanceDetail> CivilDetailedAppearanceAsync(string fileId, string appearanceId)
        {
            async Task<CivilFileDetailResponse> FileDetails() => await _filesClient.FilesCivilFileIdAsync(_requestAgencyIdentifierId, _requestPartId, fileId);
            async Task<CivilFileContent> FileContent() => await _filesClient.FilesCivilFilecontentAsync(null,null,null,null, fileId);
            async Task<CivilFileAppearancePartyResponse> AppearanceParty() => await _filesClient.FilesCivilAppearanceAppearanceIdPartiesAsync(_requestAgencyIdentifierId, _requestPartId, appearanceId);
            async Task<CivilFileAppearanceApprMethodResponse> AppearanceMethods() => await _filesClient.FilesCivilAppearanceAppearanceIdAppearancemethodsAsync(_requestAgencyIdentifierId, _requestPartId, appearanceId);
            async Task<CivilFileAppearancesResponse> Appearances() => await PopulateCivilDetailAppearancesAsync(FutureYN2.Y, HistoryYN2.Y, fileId);

            var fileDetailTask = _cache.GetOrAddAsync($"CivilFileDetail-{fileId}", FileDetails);
            var appearancePartyTask = _cache.GetOrAddAsync($"CivilAppearanceParty-{fileId}", AppearanceParty);
            var fileContentTask = _cache.GetOrAddAsync($"CivilFileContent-{fileId}", FileContent);
            var appearanceMethodsTask = _cache.GetOrAddAsync($"CivilAppearanceMethods-{fileId}", AppearanceMethods);
            var appearancesTask = _cache.GetOrAddAsync($"CivilAppearancesFull-{fileId}", Appearances);
   
            var detail = await fileDetailTask;
            var appearances = await appearancesTask;
            var agencyId = await _locationService.GetLocationAgencyIdentifier(detail?.HomeLocationAgenId);
            
            var targetAppearance = appearances?.ApprDetail?.FirstOrDefault(app => app.AppearanceId == appearanceId);
            if (targetAppearance == null || detail == null)
                return null;

   
            //Sometimes we can have a bogus location, querying court list wont work. 
            ClCivilCourtList civilCourtList = null;
            if (agencyId != null)
            {
                async Task<CourtList> CourtList() => await _filesClient.FilesCourtlistAsync(agencyId, targetAppearance.CourtRoomCd, targetAppearance.AppearanceDt, "CR", detail.FileNumberTxt);
                var courtListTask = _cache.GetOrAddAsync($"CivilCourtList-{agencyId}-{targetAppearance.CourtRoomCd}-{targetAppearance.AppearanceDt}-{detail.FileNumberTxt}", CourtList);
                var courtList = await courtListTask;
                civilCourtList = courtList.CivilCourtList.FirstOrDefault(cl => cl.AppearanceId == appearanceId);
            }

            var appearanceParty = await appearancePartyTask;
            var appearanceMethodsResponse = await appearanceMethodsTask;
            var fileContent = await fileContentTask;
            
            var appearanceDetail = appearances.ApprDetail?.FirstOrDefault(app => app.AppearanceId == appearanceId);
            var fileDetailDocuments = detail.Document.Where(doc => doc.Appearance != null && doc.Appearance.Any(app => app.AppearanceId == appearanceId)).ToList();
           
            var detailedAppearance = new CivilAppearanceDetail
            {
                PhysicalFileId = fileId,
                AgencyId = await _locationService.GetLocationAgencyIdentifier(detail.HomeLocationAgenId),
                AppearanceId = appearanceId,
                CourtRoomCd = targetAppearance.CourtRoomCd,
                FileNumberTxt = detail.FileNumberTxt,
                AppearanceDt = targetAppearance.AppearanceDt,
                AppearanceMethod = appearanceMethodsResponse.AppearanceMethod,
                Party = await PopulateCivilDetailedAppearancePartiesAsync(appearanceParty.Party, civilCourtList?.Parties), 
                OldParty = appearanceParty.Party,
                CourtListParties = civilCourtList?.Parties,
                Document = await PopulateCivilDetailedAppearanceDocuments(fileDetailDocuments, appearanceDetail)
            };
            return detailedAppearance;
        }

        public async Task<JustinReportResponse> CivilCourtSummaryReportAsync(string appearanceId, string reportName)
        {
            var justinReportResponse = await _filesClient.FilesCivilCourtsummaryreportAsync(_requestAgencyIdentifierId,
                _requestPartId, appearanceId, reportName);
            return justinReportResponse;
        }

        public async Task<CivilFileContent> CivilFileContentAsync(string agencyId, string roomCode, DateTime? proceeding, string appearanceId, string physicalFileId)
        {
            var proceedingDateString = proceeding.HasValue ? proceeding.Value.ToString("yyyy-MM-dd") : "";
            return await _filesClient.FilesCivilFilecontentAsync(agencyId, roomCode, proceedingDateString,
                appearanceId, physicalFileId);
        }

        #endregion Civil Only

        #region Criminal Only

        public async Task<FileSearchResponse> CriminalSearchAsync(FilesCriminalQuery fcq)
        {
            fcq.FilePermissions =
                "[\"A\", \"Y\", \"T\", \"F\", \"C\", \"M\", \"L\", \"R\", \"B\", \"D\", \"E\", \"G\", \"H\", \"N\", \"O\", \"P\", \"S\", \"V\"]"; // for now, use all types - TODO: determine proper list of types?

            //CourtLevel = "S"  Supreme court data, CourtLevel = "P" - Province.
            return await _filesClient.FilesCriminalAsync(_requestAgencyIdentifierId,
                _requestPartId, _requestApplicationCode, fcq.SearchMode, fcq.FileHomeAgencyId, fcq.FileNumberTxt,
                fcq.FilePrefixTxt, fcq.FilePermissions, fcq.FileSuffixNo, fcq.MdocRefTypeCode, fcq.CourtClass,
                fcq.CourtLevel, fcq.NameSearchTypeCd, fcq.LastName, fcq.OrgName, fcq.GivenName,
                fcq.Birth?.ToString("yyyy-MM-dd"), fcq.SearchByCrownPartId, fcq.SearchByCrownActiveOnly,
                fcq.SearchByCrownFileDesignation, fcq.MdocJustinNoSet, fcq.PhysicalFileIdSet);
        }

        public async Task<RedactedCriminalFileDetailResponse> CriminalFileIdAsync(string fileId)
        {
            async Task<CriminalFileDetailResponse> FileDetails() => await _filesClient.FilesCriminalFileIdAsync(_requestAgencyIdentifierId, _requestPartId, _requestApplicationCode, fileId);
            async Task<CriminalFileContent> FileContent() => await _filesClient.FilesCriminalFilecontentAsync(null, null, null, null, fileId);
            async Task<CriminalFileAppearances> FileAppearances() => await PopulateCriminalDetailsAppearancesAsync(fileId, FutureYN.Y, HistoryYN.Y);

            var fileDetailTask = _cache.GetOrAddAsync($"CriminalFileDetail-{fileId}", FileDetails);
            var fileContentTask = _cache.GetOrAddAsync($"CriminalFileContent-{fileId}", FileContent);
            var appearancesTask = _cache.GetOrAddAsync($"CriminalAppearancesFull-{fileId}", FileAppearances);

            var fileDetail = await fileDetailTask;
            var fileContent = await fileContentTask;
            var appearances = await appearancesTask;

            //CriminalFileContent can return null when an invalid fileId is inserted.
            if (fileDetail == null || fileContent == null)
                return null;

            var detail = _mapper.Map<RedactedCriminalFileDetailResponse>(fileDetail);
            var documents = PopulateCriminalDetailDocuments(fileContent);
            detail = await PopulateBaseCriminalDetail(detail);
            detail.Appearances = appearances;
            detail.Witness = await PopulateCriminalDetailWitnesses(detail);
            detail.Participant = await PopulateCriminalDetailParticipants(detail, documents, fileContent.AccusedFile);
            detail.HearingRestriction = await PopulateCriminalDetailHearingRestrictions(detail);
            detail.Crown = PopulateCriminalDetailCrown(detail);
            return detail;
        }

        public async Task<CriminalAppearanceDetail> CriminalAppearanceDetailAsync(string fileId, string appearanceId, string partId)
        {
            async Task<CriminalFileDetailResponse> FileDetails() => await _filesClient.FilesCriminalFileIdAsync(_requestAgencyIdentifierId, _requestPartId, _requestApplicationCode, fileId);
            async Task<CriminalFileContent> FileContent() => await _filesClient.FilesCriminalFilecontentAsync(null, null, null, null, fileId);
            async Task<CriminalFileAppearances> Appearances() => await PopulateCriminalDetailsAppearancesAsync(fileId, FutureYN.Y, HistoryYN.Y);
            async Task<CriminalFileAppearanceCountResponse> AppearanceCounts() => await _filesClient.FilesCriminalAppearanceAppearanceIdCountsAsync(_requestAgencyIdentifierId, _requestPartId, appearanceId);
            async Task<CriminalFileAppearanceApprMethodResponse> AppearanceMethods() => await _filesClient.FilesCriminalAppearanceAppearanceIdAppearancemethodsAsync(_requestAgencyIdentifierId, _requestPartId, appearanceId);
   
            var appearancesTask = _cache.GetOrAddAsync($"CriminalAppearancesFull-{fileId}", Appearances);
            var fileDetailTask = _cache.GetOrAddAsync($"CriminalFileDetail-{fileId}", FileDetails);
            var fileContentTask = _cache.GetOrAddAsync($"CriminalFileContent-{fileId}", FileContent);
            var appearanceCountTask = _cache.GetOrAddAsync($"CriminalAppearanceCounts-{fileId}", AppearanceCounts);
            var appearanceMethodsTask = _cache.GetOrAddAsync($"CriminalAppearanceMethods-{fileId}", AppearanceMethods);

            //Probably cached, user has the path: FileDetails -> Appearances -> AppearanceDetails. 
            var appearances = await appearancesTask;
            var detail = _mapper.Map<RedactedCriminalFileDetailResponse>(await fileDetailTask);
            var agencyId = await _locationService.GetLocationAgencyIdentifier(detail.HomeLocationAgenId);
            detail.Witness = await PopulateCriminalDetailWitnesses(detail);

            var targetAppearance = appearances?.ApprDetail?.FirstOrDefault(app => app.AppearanceId == appearanceId && app.PartId == partId);
            if (targetAppearance == null || agencyId == null)
                return null;

            async Task<CourtList> CourtList() => await _filesClient.FilesCourtlistAsync(agencyId, targetAppearance.CourtRoomCd, targetAppearance.AppearanceDt, "CR", detail.FileNumberTxt);
            var courtList = await _cache.GetOrAddAsync($"CriminalCourtList-{agencyId}-{targetAppearance.CourtRoomCd}-{targetAppearance.AppearanceDt}-{detail.FileNumberTxt}", CourtList);
            var fileContent = await fileContentTask;
            var appearanceCount = await appearanceCountTask;
            var appearanceMethods = await appearanceMethodsTask;

            var accusedFile = fileContent?.AccusedFile.FirstOrDefault(af => af.MdocJustinNo == fileId && af.PartId == partId);
            var criminalParticipant = detail?.Participant.FirstOrDefault(x => x.PartId == partId);
            var appearanceFromAccused = accusedFile?.Appearance.FirstOrDefault(a => a?.AppearanceId == appearanceId);
            var targetCourtList = courtList.CriminalCourtList.FirstOrDefault(cl => cl.CriminalAppearanceID == appearanceId);
            var attendanceMethods = targetCourtList?.AttendanceMethod;
      
            if (criminalParticipant == null || accusedFile == null || appearanceFromAccused == null)
                return null;

            //TODO: Need attendanceMethods for Witness and Respondent Not sure how to tie these back. 
            var appearanceDetail = new CriminalAppearanceDetail
            {
                JustinNo = fileId,
                AgencyId = await _locationService.GetLocationAgencyIdentifier(detail.HomeLocationAgenId),
                AppearanceId = appearanceId,
                PartId = targetAppearance.PartId,
                ProfSeqNo = targetAppearance.ProfSeqNo,
                CourtRoomCd = targetAppearance.CourtRoomCd,
                FileNumberTxt = detail.FileNumberTxt,
                AppearanceMethods = await PopulateAppearanceMethods(appearanceMethods.AppearanceMethod),
                AppearanceDt = targetAppearance.AppearanceDt,
                AppearanceNote = appearanceFromAccused.AppearanceNote?.ReturnNullIfEmpty(),
                JudgesRecommendation = appearanceFromAccused.JudgesRecommendation,
                EstimatedTimeHour = appearanceFromAccused.EstimatedTimeHour?.ReturnNullIfEmpty(),
                EstimatedTimeMin = appearanceFromAccused.EstimatedTimeMin?.ReturnNullIfEmpty(),
                Accused = await PopulateCriminalAppearanceCriminalAccused(criminalParticipant.FullName, appearanceFromAccused, attendanceMethods),
                Prosecutor = await PopulateCriminalAppearanceDetailProsecutor(appearanceFromAccused, attendanceMethods),
                Adjudicator = await PopulateCriminalAppearanceDetailAdjudicator(appearanceFromAccused, attendanceMethods),
                JustinCounsel = await PopulateCriminalAppearanceDetailJustinCounsel(criminalParticipant, appearanceFromAccused, attendanceMethods),
                Charges = await PopulateCharges(appearanceCount.ApprCount)
            };
            return appearanceDetail;
        }

        #endregion Criminal Only

        #region Courtlist & Document

        public async Task<CourtList> CourtListAsync(string agencyId, string roomCode, DateTime? proceeding, string divisionCode, string fileNumber)
        {
            var proceedingDateString = proceeding.HasValue ? proceeding.Value.ToString("yyyy-MM-dd") : "";
            return await _filesClient.FilesCourtlistAsync(agencyId, roomCode, proceedingDateString, divisionCode,
                fileNumber);
        }

        public async Task<DocumentResponse> DocumentAsync(string documentId, bool isCriminal)
        {
            return await _filesClient.FilesDocumentAsync(documentId, isCriminal ? "R" : "I");
        }

        #endregion Courtlist & Document

        #endregion Methods

        #region Helpers

        #region Criminal Details

        private async Task<CriminalFileAppearances> PopulateCriminalDetailsAppearancesAsync(string fileId, FutureYN? future, HistoryYN? history)
        {
            var criminalFileIdAppearances = await _filesClient.FilesCriminalFileIdAppearancesAsync(_requestAgencyIdentifierId, _requestPartId, future, history, fileId);
            var criminalAppearances = _mapper.Map<CriminalFileAppearances>(criminalFileIdAppearances);
            foreach (var appearance in criminalAppearances.ApprDetail)
            {
                appearance.AppearanceReasonDsc = await _lookupService.GetCriminalAppearanceReasonsDescription(appearance.AppearanceReasonCd);
                appearance.AppearanceResultDsc = await _lookupService.GetCriminalAppearanceResultsDescription(appearance.AppearanceResultCd);
                appearance.AppearanceStatusDsc = await _lookupService.GetCriminalAppearanceStatusDescription(appearance.AppearanceStatusCd.ToString());
                appearance.CourtLocationId = await _locationService.GetLocationAgencyIdentifier(appearance.CourtAgencyId);
                appearance.CourtLocation = await _locationService.GetLocationName(appearance.CourtAgencyId);
            }
            return criminalAppearances;
        }

        private string GetParticipantIdFromDetail(string partId, RedactedCriminalFileDetailResponse detail) => detail.Participant?.FirstOrDefault(p => p != null && p.PartId == partId)?.PartId;

        private List<CriminalBan> PopulateBans(CfcAccusedFile accusedFile)
        {
            var bans = _mapper.Map<List<CriminalBan>>(accusedFile.Ban.Where(b => b != null));
            bans.ForEach(b => b.PartId = accusedFile.PartId);
            return bans;
        }

        private async Task<List<CriminalCount>> PopulateCounts(CfcAccusedFile accusedFile, RedactedCriminalFileDetailResponse detail)
        {
            var criminalCount = new List<CriminalCount>();
            foreach (var appearance in accusedFile.Appearance.Where(a => a != null))
            {
                foreach (var count in _mapper.Map<ICollection<CriminalCount>>(appearance.AppearanceCount.Where(a => a?.AppearanceResult == "END")))
                {
                    count.PartId = GetParticipantIdFromDetail(accusedFile.PartId, detail);
                    count.AppearanceDate = appearance.AppearanceDate;
                    count.Sentence = count.Sentence.Where(s => s != null).ToList();
                    foreach (var criminalSentence in count.Sentence)
                    {
                        criminalSentence.JudgesRecommendation = appearance.JudgesRecommendation;
                        criminalSentence.SentenceTypeDesc = await _lookupService.GetCriminalSentenceDescription(criminalSentence.SntpCd);
                    }
                    count.FindingDsc = await _lookupService.GetFindingDescription(count.Finding);
                    criminalCount.Add(count);
                }
            }
            return criminalCount;
        }

        private List<CriminalDocument> PopulateCriminalDetailDocuments(CriminalFileContent criminalFileContent)
        {
            return criminalFileContent.AccusedFile.SelectMany(ac =>
            {
                var criminalDocuments = _mapper.Map<List<CriminalDocument>>(ac.Document);

                //Create ROPs.
                if (ac.Appearance != null && ac.Appearance.Any())
                {
                    criminalDocuments.Insert(0, new CriminalDocument
                    {
                        DocumentTypeDescription = "Record of Proceedings",
                        ImageId = ac.PartId,
                        Category = "rop",
                        PartId = ac.PartId,
                        HasFutureAppearance = ac.Appearance?.Any(a =>
                            a?.AppearanceDate != null && DateTime.Parse(a.AppearanceDate) >= DateTime.Today)
                    });
                }

                //Populate extra fields.
                foreach (var document in criminalDocuments)
                {
                    document.Category = string.IsNullOrEmpty(document.Category) ? _lookupService.GetDocumentCategory(document.DocmFormId) : document.Category;
                    document.DocumentTypeDescription = document.DocmFormDsc;
                    document.PartId = string.IsNullOrEmpty(ac.PartId) ? null : ac.PartId;
                    document.DocmId = string.IsNullOrEmpty(document.DocmId) ? null : document.DocmId;
                    document.ImageId = string.IsNullOrEmpty(document.ImageId) ? null : document.ImageId;
                }
                return criminalDocuments;
            }).ToList();
        }

        private async Task<ICollection<CriminalWitness>> PopulateCriminalDetailWitnesses(RedactedCriminalFileDetailResponse detail)
        {
            foreach (var witness in detail.Witness)
            {
                witness.AgencyCd = await _lookupService.GetAgencyLocationCode(witness.AgencyId);
                witness.AgencyDsc = await _lookupService.GetAgencyLocationDescription(witness.AgencyId);
                witness.WitnessTypeDsc = await _lookupService.GetWitnessRoleTypeDescription(witness.WitnessTypeCd);
            }
            return detail.Witness;
        }

        private async Task<ICollection<CriminalParticipant>> PopulateCriminalDetailParticipants(RedactedCriminalFileDetailResponse detail, ICollection<CriminalDocument> documents, ICollection<CfcAccusedFile> accusedFiles)
        {
            foreach (var participant in detail.Participant)
            {
                participant.Document = documents.Where(doc => doc.PartId == participant.PartId).ToList();
                participant.HideJustinCounsel = false;   //TODO tie this to a permission. View Witness List permission
                //TODO COUNSEL? This would have to come from law society data, which is stored in a CSV file. 
                foreach (var accusedFile in accusedFiles.Where(af => af?.PartId == participant.PartId))
                {
                    participant.Count.AddRange(await PopulateCounts(accusedFile, detail));
                    participant.Ban.AddRange(PopulateBans(accusedFile));
                }
            }
            return detail.Participant;
        }

        private async Task<RedactedCriminalFileDetailResponse> PopulateBaseCriminalDetail(RedactedCriminalFileDetailResponse detail)
        {
            detail.HomeLocationAgencyName = await _locationService.GetLocationName(detail.HomeLocationAgenId);
            detail.HomeLocationAgencyCode = await _locationService.GetLocationAgencyIdentifier(detail.HomeLocationAgenId);
            detail.HomeLocationRegionName = await _locationService.GetRegionName(detail.HomeLocationAgencyCode);
            detail.CourtClassDescription = await _lookupService.GetCourtClassDescription(detail.CourtClassCd.ToString());
            detail.CourtLevelDescription = await _lookupService.GetCourtLevelDescription(detail.CourtLevelCd.ToString());
            detail.ActivityClassCd = await _lookupService.GetActivityClassCd(detail.CourtClassCd.ToString());
            detail.CrownEstimateLenDsc = detail.CrownEstimateLenUnit.HasValue ? await _lookupService.GetAppearanceDuration(detail.CrownEstimateLenUnit.Value.ToString()) : null;
            detail.AssignmentTypeDsc = await _lookupService.GetComplexityTypeDescription(detail.ComplexityTypeCd?.ToString());
            return detail;
        }

        private async Task<ICollection<CriminalHearingRestriction>> PopulateCriminalDetailHearingRestrictions(RedactedCriminalFileDetailResponse detail)
        {
            foreach (var hearingRestriction in detail.HearingRestriction)
                hearingRestriction.HearingRestrictionTypeDsc = await _lookupService.GetHearingRestrictionDescription(hearingRestriction.HearingRestrictionTypeCd.ToString());
            return detail.HearingRestriction.Where(hr => hr.HearingRestrictionTypeCd == HearingRestriction2HearingRestrictionTypeCd.S).ToList(); //TODO conditional permission. MY_CALENDAR_SEIZED_AARS)
        }

        private ICollection<CrownWitness> PopulateCriminalDetailCrown(RedactedCriminalFileDetailResponse detail) => _mapper.Map<ICollection<CrownWitness>>(detail.Witness.Where(w => w.RoleTypeCd == CriminalWitnessRoleTypeCd.CRN).ToList());

        #endregion Criminal Details

        #region Criminal Appearance Details

        private async Task<CriminalAccused> PopulateCriminalAppearanceCriminalAccused(string fullName, CfcAppearance appearanceFromAccused, ICollection<ClAttendanceMethod> attendanceMethods)
        {
            var partyAppearanceMethod = appearanceFromAccused?.PartyAppearanceMethod.FirstOrDefault(pam => pam.PartyRole == "ACC");
            var attendanceMethod = attendanceMethods?.FirstOrDefault(am => am.RoleType == "ACC");
            return new CriminalAccused
            {
                FullName = fullName,
                PartId = partyAppearanceMethod?.PartId,
                PartyAppearanceMethod = partyAppearanceMethod?.PartyAppearanceMethod,
                PartyAppearanceMethodDesc = null,  //TODO waiting for correct code lookup, I don't seem to have
                AttendanceMethodCd = attendanceMethod?.AttendanceMethodCd,
                AttendanceMethodDesc = await _lookupService.GetCriminalAssetsDescriptions(attendanceMethod?.AttendanceMethodCd)
            };
        }

        private async Task<Adjudicator> PopulateCriminalAppearanceDetailAdjudicator(CfcAppearance appearanceFromAccused, ICollection<ClAttendanceMethod> attendanceMethods)
        {
            var partyAppearanceMethod = appearanceFromAccused?.PartyAppearanceMethod.FirstOrDefault(pam => pam.PartyRole == "ADJ");
            var attendanceMethod = attendanceMethods?.FirstOrDefault(am => am.RoleType == "ADJ");
            if (partyAppearanceMethod == null)
                return null;

            return new Adjudicator
            {
                FullName = partyAppearanceMethod.PartyName.ConvertNameLastCommaFirstToFirstLast(),
                PartId = partyAppearanceMethod.PartId,
                PartyAppearanceMethod = partyAppearanceMethod.PartyAppearanceMethod,
                PartyAppearanceMethodDesc = null, //TODO waiting for correct code lookup, I don't seem to have
                AttendanceMethodCd = attendanceMethod?.AttendanceMethodCd,
                AttendanceMethodDesc = await _lookupService.GetCriminalAssetsDescriptions(attendanceMethod?.AttendanceMethodCd)
            };
        }

        private async Task<Prosecutor> PopulateCriminalAppearanceDetailProsecutor(CfcAppearance appearanceFromAccused, ICollection<ClAttendanceMethod> attendanceMethods)
        {
            var partyAppearanceMethod = appearanceFromAccused?.PartyAppearanceMethod.FirstOrDefault(pam => pam.PartyRole == "PRO");
            var attendanceMethod = attendanceMethods?.FirstOrDefault(am => am.RoleType == "PRO");
            if (partyAppearanceMethod == null)
                return null;

            return new Prosecutor
            {
                FullName = partyAppearanceMethod.PartyName.ConvertNameLastCommaFirstToFirstLast(),
                PartId = partyAppearanceMethod.PartId,
                PartyAppearanceMethod = partyAppearanceMethod.PartyAppearanceMethod,
                PartyAppearanceMethodDesc = null,  //TODO waiting for correct code lookup, I don't seem to have
                AttendanceMethodCd = attendanceMethod?.AttendanceMethodCd,
                AttendanceMethodDesc = await _lookupService.GetCriminalAssetsDescriptions(attendanceMethod?.AttendanceMethodCd)
            };
        }

        private async Task<JustinCounsel> PopulateCriminalAppearanceDetailJustinCounsel(CriminalParticipant criminalParticipant, CfcAppearance appearanceFromAccused, ICollection<ClAttendanceMethod> attendanceMethods)
        {
            if (criminalParticipant == null)
                return null;

            var justinCounsel = _mapper.Map<JustinCounsel>(criminalParticipant);
            var partyAppearanceMethod = appearanceFromAccused?.PartyAppearanceMethod.FirstOrDefault(pam => pam.PartyRole == "CON");
            var attendanceMethod = attendanceMethods?.FirstOrDefault(am => am.RoleType == "CON");
            justinCounsel.PartyAppearanceMethod = partyAppearanceMethod?.PartyAppearanceMethod;
            justinCounsel.PartyAppearanceMethodDesc = null;  //TODO waiting for correct code lookup, I don't seem to have
            justinCounsel.AttendanceMethodCd = attendanceMethod?.AttendanceMethodCd;
            justinCounsel.AttendanceMethodDesc = await _lookupService.GetCriminalAssetsDescriptions(attendanceMethod?.AttendanceMethodCd);
            //justinCounsel.PartyAppearanceMethodDesc = await _lookupService.GetCriminalAppearanceMethodAccusedCounsel(partyAppearanceMethod?.PartyAppearanceMethod);
            //We could assign name here from the PartyAppearance, but that doesn't appear to be correct. 
            //While testing I found data inside of PartyAppearances, but used CourtList and saw there was no assigned counsel. 
            //FileId 1180, AppearanceId 1528.0026, PartId 15911.0026 is an example of this on the Development Environment. 
            return justinCounsel;
        }

        public async Task<CriminalFileContent> CriminalFileContentAsync(string agencyId, string roomCode, DateTime? proceeding, string appearanceId, string justinNumber)
        {
            var proceedingDateString = proceeding.HasValue ? proceeding.Value.ToString("yyyy-MM-dd") : "";
            return await _filesClient.FilesCriminalFilecontentAsync(agencyId, roomCode,
                proceedingDateString, appearanceId, justinNumber);
        }

        public async Task<RopResponse> RecordOfProceedingsAsync(string partId, string profSequenceNumber, CourtLevelCd courtLevelCode, CourtClassCd courtClassCode)
        {
            return await _filesClient.FilesRecordOfProceedingsAsync(partId, profSequenceNumber, courtLevelCode, courtClassCode);
        }

        private async Task<ICollection<CriminalCharges>> PopulateCharges(ICollection<CriminalAppearanceCount> appearanceCount)
        {
            var charges = _mapper.Map<ICollection<CriminalCharges>>(appearanceCount);
            //Populate charges or counts extra fields.
            foreach (var charge in charges)
            {
                charge.AppearanceReasonDsc = await _lookupService.GetCriminalAppearanceReasonsDescription(charge.AppearanceReasonCd);
                charge.AppearanceResultDesc = await _lookupService.GetCriminalAppearanceResultsDescription(charge.AppearanceResultCd);
                charge.FindingDsc = await _lookupService.GetFindingDescription(charge.FindingCd);
            }
            return charges;
        }

        private async Task<ICollection<CriminalAppearanceMethod>> PopulateAppearanceMethods(ICollection<JCCommon.Clients.FileServices.CriminalAppearanceMethod> appearanceMethods)
        {
            var criminalAppearanceMethods = _mapper.Map<ICollection<CriminalAppearanceMethod>>(appearanceMethods);
            //Populate appearance methods extra fields.
            foreach (var appearanceMethod in criminalAppearanceMethods)
            {
                appearanceMethod.AppearanceMethodDesc = await _lookupService.GetCriminalAssetsDescriptions(appearanceMethod.AppearanceMethodCd);
                appearanceMethod.RoleTypeDsc = await _lookupService.GetCriminalParticipantRoleDescription(appearanceMethod.RoleTypeCd); // double check this one. 
            }
            return criminalAppearanceMethods;
        }

        #endregion Criminal Appearance Details

        #region Civil Details

        private async Task<CivilFileAppearancesResponse> PopulateCivilDetailAppearancesAsync(FutureYN2? future, HistoryYN2? history, string fileId)
        {
            var civilFileAppearancesResponse = await _filesClient.FilesCivilFileIdAppearancesAsync(_requestAgencyIdentifierId, _requestPartId, future, history,
                fileId);
            if (civilFileAppearancesResponse == null)
                return null;

            var civilAppearances = _mapper.Map<CivilFileAppearances>(civilFileAppearancesResponse);
            foreach (var appearance in civilAppearances.ApprDetail)
            {
                appearance.AppearanceReasonDsc = await _lookupService.GetCivilAppearanceReasonsDescription(appearance.AppearanceReasonCd);
                appearance.AppearanceResultDsc = await _lookupService.GetCivilAppearanceResultsDescription(appearance.AppearanceResultCd);
                appearance.AppearanceStatusDsc = await _lookupService.GetCivilAppearanceStatusDescription(appearance.AppearanceStatusCd.ToString());
                appearance.CourtLocationId = await _locationService.GetLocationAgencyIdentifier(appearance.CourtAgencyId);
                appearance.CourtLocation = await _locationService.GetLocationName(appearance.CourtAgencyId);
                appearance.DocumentTypeDsc = await _lookupService.GetDocumentDescriptionAsync(appearance.DocumentTypeCd);
            }
            return civilAppearances;
        }

        private IEnumerable<CivilDocument> PopulateCivilDetailCsrsDocuments(ICollection<CvfcAppearance> appearances)
        {
            //Add in CSRs.
            return appearances.Select(appearance => new CivilDocument()
            {
                Category = "CSR",
                DocumentTypeDescription = "Court Summary",
                CivilDocumentId = appearance.AppearanceId,
                ImageId = appearance.AppearanceId,
                DocumentTypeCd = "CSR",
                LastAppearanceDt = appearance.AppearanceDate,
                FiledDt = appearance.AppearanceDate,
            });
        }

        private async Task<RedactedCivilFileDetailResponse> PopulateBaseCivilDetail(RedactedCivilFileDetailResponse detail)
        {
            detail.HomeLocationAgencyCode = await _locationService.GetLocationAgencyIdentifier(detail.HomeLocationAgenId);
            detail.HomeLocationAgencyName = await _locationService.GetLocationName(detail.HomeLocationAgenId);
            detail.HomeLocationRegionName = await _locationService.GetRegionName(detail.HomeLocationAgencyCode);
            detail.CourtClassDescription = await _lookupService.GetCourtClassDescription(detail.CourtClassCd.ToString());
            detail.CourtLevelDescription = await _lookupService.GetCourtLevelDescription(detail.CourtLevelCd.ToString());
            detail.ActivityClassCd = await _lookupService.GetActivityClassCd(detail.CourtClassCd.ToString());
            return detail;
        }

        private async Task<ICollection<CivilDocument>> PopulateCivilDetailDocuments(ICollection<CivilDocument> documents)
        {
            //TODO permission for documents.
            //Populate extra fields for document.
            foreach (var document in documents.Where(doc => doc.Category != "CSR"))
            {
                document.Category = _lookupService.GetDocumentCategory(document.DocumentTypeCd);
                document.DocumentTypeDescription = await _lookupService.GetDocumentDescriptionAsync(document.DocumentTypeCd);
                document.ImageId = document.SealedYN != "N" ? null : document.ImageId;
                document.Appearance = null;
            }
            return documents;
        }

        private async Task<ICollection<CivilParty>> PopulateCivilDetailParties(ICollection<CivilParty> parties)
        {
            //Populate extra fields for party.
            foreach (var party in parties)
                party.RoleTypeDescription = await _lookupService.GetCivilRoleTypeDescription(party.RoleTypeCd);
            return parties;
        }

        private async Task<ICollection<CivilHearingRestriction>> PopulateCivilDetailHearingRestrictions(ICollection<CvfcHearingRestriction2> hearingRestrictions)
        {
            //TODO need permission for this filter.
            var civilHearingRestrictions = _mapper.Map<ICollection<CivilHearingRestriction>>(hearingRestrictions.Where(hr =>
                    hr.HearingRestrictionTypeCd == CvfcHearingRestriction2HearingRestrictionTypeCd.S)
                .ToList());

            foreach (var hearing in civilHearingRestrictions)
            {
                hearing.HearingRestrictionTypeDsc = await _lookupService.GetHearingRestrictionDescription(hearing.HearingRestrictionTypeCd.ToString());
            }
            return civilHearingRestrictions;
        }

        #endregion Civil Details

        #region Civil Appearance Details
        /// <summary>
        /// This is mostly based off of getAppearanceCivilParty and expands by court list. 
        /// </summary>
        /// <param name="parties"></param>
        /// <param name="courtListParties"></param>
        /// <returns></returns>
        private async Task<ICollection<CivilAppearanceDetailParty>> PopulateCivilDetailedAppearancePartiesAsync(ICollection<CivilAppearanceParty> parties, ICollection<ClParty> courtListParties)
        {
            var resultParties = new List<CivilAppearanceDetailParty>();
            foreach ( var partyGroup in parties.GroupBy(a=> a.PartyId))
            {
                //Map over our primary values from party group. 
                var party = _mapper.Map<CivilAppearanceDetailParty>(partyGroup.First());

                //Get our roles from getAppearanceCivilParty. These should essentially be the same.
                party.PartyRole = await partyGroup.Select(async pg => new ClPartyRole
                {
                    RoleTypeCd = pg.PartyRoleTypeCd,
                    RoleTypeDsc = await _lookupService.GetCivilRoleTypeDescription(pg.PartyRoleTypeCd)
                }).WhenAll();

                //Get the additional information from court list. 
                var courtListParty = courtListParties?.FirstOrDefault(clp => clp.PartyId == partyGroup.Key);
                if (courtListParty != null)
                {
                    party.AttendanceMethodCd = courtListParty.AttendanceMethodCd ?? "P"; //TODO double check this, supposidly if there is no value, it's assumed as present.
                    party.AttendanceMethodDesc = await _lookupService.GetCivilAssetsDescription(party.AttendanceMethodCd);
                    party.Counsel = courtListParty.Counsel;
                    party.Representative = courtListParty.Representative;
                    party.LegalRepresentative = courtListParty.LegalRepresentative;
                }
                resultParties.Add(party);
            }
            return resultParties;
        }

        private async Task<ICollection<CivilAppearanceDocument>> PopulateCivilDetailedAppearanceDocuments(List<CvfcDocument3> fileDetailDocuments, JCCommon.Clients.FileServices.CivilAppearanceDetail appearance)
        {
            //CivilAppearanceDocument, doesn't include appearances.
            var documents = _mapper.Map<ICollection<CivilAppearanceDocument>>(fileDetailDocuments);
            foreach (var document in documents)
            {
                document.Category = _lookupService.GetDocumentCategory(document.DocumentTypeCd);
                document.DocumentTypeDescription = await _lookupService.GetDocumentDescriptionAsync(document.DocumentTypeCd);
                document.AppearanceResultCd = appearance?.AppearanceResultCd;
                document.AppearanceResultDesc = await _lookupService.GetCriminalAppearanceResultsDescription(appearance?.AppearanceResultCd);
            }
            return documents;
        }

        #endregion Civil Appearance Details

        #endregion Helpers
    }
}