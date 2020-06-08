using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JCCommon.Clients.FileServices;
using JCCommon.Models;
using LazyCache;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Serialization;
using Scv.Api.Helpers;
using Scv.Api.Helpers.ContractResolver;
using Scv.Api.Helpers.Extensions;
using Scv.Api.Models.Criminal.AppearanceDetail;
using Scv.Api.Models.Criminal.Appearances;
using Scv.Api.Models.Criminal.Detail;
using CriminalAppearanceDetail = Scv.Api.Models.Criminal.AppearanceDetail.CriminalAppearanceDetail;
using CriminalAppearanceMethod = Scv.Api.Models.Criminal.AppearanceDetail.CriminalAppearanceMethod;
using CriminalParticipant = Scv.Api.Models.Criminal.Detail.CriminalParticipant;
using CriminalWitness = Scv.Api.Models.Criminal.Detail.CriminalWitness;

namespace Scv.Api.Services
{
    public class CriminalFilesService
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

        #endregion

        #region Constructor

        public CriminalFilesService(IConfiguration configuration, FileServicesClient filesClient, IMapper mapper, LookupService lookupService, LocationService locationService, IAppCache cache)
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
        }

        #endregion Constructor


        #region Methods
        public async Task<FileSearchResponse> SearchAsync(FilesCriminalQuery fcq)
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

        public async Task<RedactedCriminalFileDetailResponse> FileIdAsync(string fileId)
        {
            async Task<CriminalFileDetailResponse> FileDetails() => await _filesClient.FilesCriminalFileIdAsync(_requestAgencyIdentifierId, _requestPartId, _requestApplicationCode, fileId);
            async Task<CriminalFileContent> FileContent() => await _filesClient.FilesCriminalFilecontentAsync(null, null, null, null, fileId);
            async Task<CriminalFileAppearances> FileAppearances() => await PopulateDetailsAppearancesAsync(fileId, FutureYN.Y, HistoryYN.Y);

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
            var documents = PopulateDetailDocuments(fileContent);
            detail = await PopulateBaseDetail(detail);
            detail.Appearances = appearances;
            detail.Witness = await PopulateDetailWitnesses(detail);
            detail.Participant = await PopulateDetailParticipants(detail, documents, fileContent.AccusedFile);
            detail.HearingRestriction = await PopulateCriminalDetailHearingRestrictions(detail);
            detail.Crown = PopulateDetailCrown(detail);
            return detail;
        }

        public async Task<CriminalAppearanceDetail> AppearanceDetailAsync(string fileId, string appearanceId, string partId)
        {
            async Task<CriminalFileDetailResponse> FileDetails() => await _filesClient.FilesCriminalFileIdAsync(_requestAgencyIdentifierId, _requestPartId, _requestApplicationCode, fileId);
            async Task<CriminalFileContent> FileContent() => await _filesClient.FilesCriminalFilecontentAsync(null, null, null, null, fileId);
            async Task<CriminalFileAppearances> Appearances() => await PopulateDetailsAppearancesAsync(fileId, FutureYN.Y, HistoryYN.Y);
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
            detail.Witness = await PopulateDetailWitnesses(detail);

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
                Accused = await PopulateAppearanceCriminalAccused(criminalParticipant.FullName, appearanceFromAccused, attendanceMethods),
                Prosecutor = await PopulateAppearanceDetailProsecutor(appearanceFromAccused, attendanceMethods),
                Adjudicator = await PopulateAppearanceDetailAdjudicator(appearanceFromAccused, attendanceMethods),
                JustinCounsel = await PopulateAppearanceDetailJustinCounsel(criminalParticipant, appearanceFromAccused, attendanceMethods),
                Charges = await PopulateCharges(appearanceCount.ApprCount)
            };
            return appearanceDetail;
        }

        #endregion

        #region Helpers

        #region Criminal Details

        private async Task<CriminalFileAppearances> PopulateDetailsAppearancesAsync(string fileId, FutureYN? future, HistoryYN? history)
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

        private List<CriminalDocument> PopulateDetailDocuments(CriminalFileContent criminalFileContent)
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

        private async Task<ICollection<CriminalWitness>> PopulateDetailWitnesses(RedactedCriminalFileDetailResponse detail)
        {
            foreach (var witness in detail.Witness)
            {
                witness.AgencyCd = await _lookupService.GetAgencyLocationCode(witness.AgencyId);
                witness.AgencyDsc = await _lookupService.GetAgencyLocationDescription(witness.AgencyId);
                witness.WitnessTypeDsc = await _lookupService.GetWitnessRoleTypeDescription(witness.WitnessTypeCd);
            }
            return detail.Witness;
        }

        private async Task<ICollection<CriminalParticipant>> PopulateDetailParticipants(RedactedCriminalFileDetailResponse detail, ICollection<CriminalDocument> documents, ICollection<CfcAccusedFile> accusedFiles)
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

        private async Task<RedactedCriminalFileDetailResponse> PopulateBaseDetail(RedactedCriminalFileDetailResponse detail)
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

        private ICollection<CrownWitness> PopulateDetailCrown(RedactedCriminalFileDetailResponse detail) => _mapper.Map<ICollection<CrownWitness>>(detail.Witness.Where(w => w.RoleTypeCd == CriminalWitnessRoleTypeCd.CRN).ToList());

        #endregion Criminal Details

        #region Criminal Appearance Details

        private async Task<CriminalAccused> PopulateAppearanceCriminalAccused(string fullName, CfcAppearance appearanceFromAccused, ICollection<ClAttendanceMethod> attendanceMethods)
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

        private async Task<Adjudicator> PopulateAppearanceDetailAdjudicator(CfcAppearance appearanceFromAccused, ICollection<ClAttendanceMethod> attendanceMethods)
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

        private async Task<Prosecutor> PopulateAppearanceDetailProsecutor(CfcAppearance appearanceFromAccused, ICollection<ClAttendanceMethod> attendanceMethods)
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

        private async Task<JustinCounsel> PopulateAppearanceDetailJustinCounsel(CriminalParticipant criminalParticipant, CfcAppearance appearanceFromAccused, ICollection<ClAttendanceMethod> attendanceMethods)
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

        public async Task<CriminalFileContent> FileContentAsync(string agencyId, string roomCode, DateTime? proceeding, string appearanceId, string justinNumber)
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

        #endregion

    }
}
