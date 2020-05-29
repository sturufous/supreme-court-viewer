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
using CivilAppearanceDetail = Scv.Api.Models.Civil.AppearanceDetail.CivilAppearanceDetail;
using CriminalAppearanceDetail = Scv.Api.Models.Criminal.AppearanceDetail.CriminalAppearanceDetail;
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
        private readonly FileServicesClient _fileServicesClient;
        private readonly IMapper _mapper;
        private readonly LookupService _lookupService;
        private readonly LocationService _locationService;
        private readonly string _requestApplicationCode;
        private readonly string _requestAgencyIdentifierId;
        private readonly string _requestPartId;

        #endregion Variables

        #region Constructor

        public FilesService(IConfiguration configuration, FileServicesClient fileServicesClient, IMapper mapper, LookupService lookupService, LocationService locationService, IAppCache cache)
        {
            _fileServicesClient = fileServicesClient;
            _fileServicesClient.JsonSerializerSettings.ContractResolver = new SafeContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
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

        public async Task<FileSearchResponse> FilesCivilAsync(FilesCivilQuery fcq)
        {
            fcq.FilePermissions =
                "[\"A\", \"Y\", \"T\", \"F\", \"C\", \"M\", \"L\", \"R\", \"B\", \"D\", \"E\", \"G\", \"H\", \"N\", \"O\", \"P\", \"S\", \"V\"]"; // for now, use all types - TODO: determine proper list of types?
            return await _fileServicesClient.FilesCivilAsync(_requestAgencyIdentifierId, _requestPartId,
                _requestApplicationCode, fcq.SearchMode, fcq.FileHomeAgencyId, fcq.FileNumber, fcq.FilePrefix,
                fcq.FilePermissions, fcq.FileSuffixNumber, fcq.MDocReferenceTypeCode, fcq.CourtClass, fcq.CourtLevel,
                fcq.NameSearchType, fcq.LastName, fcq.OrgName, fcq.GivenName, fcq.Birth?.ToString("yyyy-MM-dd"),
                fcq.SearchByCrownPartId, fcq.SearchByCrownActiveOnly, fcq.SearchByCrownFileDesignation,
                fcq.MdocJustinNumberSet, fcq.PhysicalFileIdSet);
        }

        public async Task<RedactedCivilFileDetailResponse> FilesCivilFileIdAsync(string fileId)
        {
            var civilFileDetailResponseTask = _cache.GetOrAddAsync($"CivilFileDetail-{fileId}",
                async () => await _fileServicesClient.FilesCivilFileIdAsync(_requestAgencyIdentifierId, _requestPartId, fileId));
            var civilAppearancesTask = _cache.GetOrAddAsync($"CivilAppearancesFull-{fileId}",
                async () => await PopulateCivilDetailAppearancesAsync(FutureYN2.Y, HistoryYN2.Y, fileId));

            var civilFileDetailResponse = await civilFileDetailResponseTask;
            var civilAppearances = await civilAppearancesTask;

            var detail = _mapper.Map<RedactedCivilFileDetailResponse>(civilFileDetailResponse);
            foreach (var document in PopulateCivilDetailCsrsDocuments(civilFileDetailResponse.Appearance))
                detail.Document.Add(document);

            detail = await PopulateBaseCivilDetail(detail);
            detail.Appearances = civilAppearances;
            detail.Party = await PopulateCivilDetailParties(detail.Party);
            detail.Document = await PopulateCivilDetailDocuments(detail.Document);
            detail.HearingRestriction = await PopulateCivilDetailHearingRestrictions(civilFileDetailResponse.HearingRestriction);
            return detail;
        }

        public async Task<CivilAppearanceDetail> FilesCivilDetailedAppearanceAsync(string fileId, string appearanceId)
        {
            var fileDetailResponseTask = _cache.GetOrAddAsync($"CivilFileDetail-{fileId}",
                async () => await _fileServicesClient.FilesCivilFileIdAsync(_requestAgencyIdentifierId, _requestPartId, fileId));
            var appearancePartyResponseTask = _fileServicesClient.FilesCivilAppearanceAppearanceIdPartiesAsync(_requestAgencyIdentifierId, _requestPartId, appearanceId);
            var appearanceMethodsResponseTask = _fileServicesClient.FilesCivilAppearanceAppearanceIdAppearancemethodsAsync(_requestAgencyIdentifierId, _requestPartId, appearanceId);

            var fileDetailResponse = await fileDetailResponseTask;
            var appearancePartyResponse = await appearancePartyResponseTask;
            var appearanceMethodsResponse = await appearanceMethodsResponseTask;

            var documentsWithSameAppearanceId = fileDetailResponse.Document.Where(doc =>
                    doc.Appearance != null && doc.Appearance.Any(app => app.AppearanceId == appearanceId))
                .ToList();

            var detailedAppearance = new CivilAppearanceDetail
            {
                PhysicalFileId = fileId,
                AppearanceMethod = appearanceMethodsResponse.AppearanceMethod,
                Party = await PopulateCivilDetailedAppearanceParties(appearancePartyResponse.Party),
                Document = await PopulateCivilDetailedAppearanceDocuments(documentsWithSameAppearanceId)
            };
            return detailedAppearance;
        }

        public async Task<JustinReportResponse> FilesCivilCourtsummaryreportAsync(string appearanceId, string reportName)
        {
            var justinReportResponse = await _fileServicesClient.FilesCivilCourtsummaryreportAsync(_requestAgencyIdentifierId,
                _requestPartId, appearanceId, reportName);
            return justinReportResponse;
        }

        public async Task<CivilFileContent> FilesCivilFilecontentAsync(string agencyId, string roomCode, DateTime? proceeding, string appearanceId, string physicalFileId)
        {
            var proceedingDateString = proceeding.HasValue ? proceeding.Value.ToString("yyyy-MM-dd") : "";
            return await _fileServicesClient.FilesCivilFilecontentAsync(agencyId, roomCode, proceedingDateString,
                appearanceId, physicalFileId);
        }

        #endregion Civil Only

        #region Criminal Only

        public async Task<FileSearchResponse> FilesCriminalAsync(FilesCriminalQuery fcq)
        {
            fcq.FilePermissions =
                "[\"A\", \"Y\", \"T\", \"F\", \"C\", \"M\", \"L\", \"R\", \"B\", \"D\", \"E\", \"G\", \"H\", \"N\", \"O\", \"P\", \"S\", \"V\"]"; // for now, use all types - TODO: determine proper list of types?

            //CourtLevel = "S"  Supreme court data, CourtLevel = "P" - Province.
            return await _fileServicesClient.FilesCriminalAsync(_requestAgencyIdentifierId,
                _requestPartId, _requestApplicationCode, fcq.SearchMode, fcq.FileHomeAgencyId, fcq.FileNumberTxt,
                fcq.FilePrefixTxt, fcq.FilePermissions, fcq.FileSuffixNo, fcq.MdocRefTypeCode, fcq.CourtClass,
                fcq.CourtLevel, fcq.NameSearchTypeCd, fcq.LastName, fcq.OrgName, fcq.GivenName,
                fcq.Birth?.ToString("yyyy-MM-dd"), fcq.SearchByCrownPartId, fcq.SearchByCrownActiveOnly,
                fcq.SearchByCrownFileDesignation, fcq.MdocJustinNoSet, fcq.PhysicalFileIdSet);
        }

        public async Task<RedactedCriminalFileDetailResponse> FilesCriminalFileIdAsync(string fileId)
        {
            var criminalFileDetailTask = _cache.GetOrAddAsync($"CriminalFileDetail-{fileId}",
                async () => await _fileServicesClient.FilesCriminalFileIdAsync(_requestAgencyIdentifierId,
                    _requestPartId, _requestApplicationCode, fileId));
            var criminalFileContentTask = _cache.GetOrAddAsync($"CriminalFileContent-{fileId}",
                async () => await _fileServicesClient.FilesCriminalFilecontentAsync(null, null, null, null, fileId));
            var criminalAppearancesTask = _cache.GetOrAddAsync($"CriminalAppearancesFull-{fileId}",
                async () => await PopulateCriminalDetailsAppearancesAsync(fileId, FutureYN.Y, HistoryYN.Y));

            var criminalFileDetail = await criminalFileDetailTask;
            var criminalFileContent = await criminalFileContentTask;
            var criminalAppearances = await criminalAppearancesTask;

            //CriminalFileContent can return null when an invalid fileId is inserted.
            if (criminalFileDetail == null || criminalFileContent == null)
                return null;

            var detail = _mapper.Map<RedactedCriminalFileDetailResponse>(criminalFileDetail);
            var documents = PopulateCriminalDetailDocuments(criminalFileContent);
            detail = await PopulateBaseCriminalDetail(detail);
            detail.Appearances = criminalAppearances;
            detail.Witness = await PopulateCriminalDetailWitnesses(detail);
            detail.Participant = await PopulateCriminalDetailParticipants(detail, documents, criminalFileContent.AccusedFile);
            detail.HearingRestriction = await PopulateCriminalDetailHearingRestrictions(detail);
            detail.Crown = PopulateCriminalDetailCrown(detail);
            return detail;
        }

        public async Task<CriminalAppearanceDetail> FilesCriminalAppearanceDetailAsync(string fileId, string appearanceId, string partId = null, string profSeqNo = null)
        {
            var fileDetailTask = _cache.GetOrAddAsync($"CriminalFileDetail-{fileId}",
                async () => await _fileServicesClient.FilesCriminalFileIdAsync(_requestAgencyIdentifierId,
                    _requestPartId, _requestApplicationCode, fileId));
            var appearanceCountTask = _fileServicesClient.FilesCriminalAppearanceAppearanceIdCountsAsync(_requestAgencyIdentifierId, _requestPartId, appearanceId);
            var appearanceMethodsTask = _fileServicesClient.FilesCriminalAppearanceAppearanceIdAppearancemethodsAsync(_requestAgencyIdentifierId, _requestPartId, appearanceId);
            var fileContentTask = _cache.GetOrAddAsync($"CriminalFileContent-{fileId}",
                async () => await _fileServicesClient.FilesCriminalFilecontentAsync(null, null, null, null, fileId));

            var fileDetail = await fileDetailTask;
            var appearanceCount = await appearanceCountTask;
            var appearanceMethods = await appearanceMethodsTask;
            var fileContent = await fileContentTask;

            var redactedDetail = _mapper.Map<RedactedCriminalFileDetailResponse>(fileDetail);
            var accused = redactedDetail.Participant.FirstOrDefault(x => x.PartId == partId && x.ProfSeqNo == profSeqNo);
            var accusedFile = fileContent.AccusedFile.FirstOrDefault(af => af.MdocJustinNo == fileId);
            var appearanceDetail = new CriminalAppearanceDetail
            {
                JustinNo = fileId,
                PartId = partId,
                ProfSeqNo = profSeqNo,
                JustinCounsel = accused != null ? _mapper.Map<JustinCounsel>(accused) : null,
                Charges = await PopulateCharges(appearanceCount.ApprCount),
                AppearanceMethods = await PopulateAppearanceMethods(appearanceMethods.AppearanceMethod),
                AppearanceNote = accusedFile?.Appearance.FirstOrDefault(a=> a.AppearanceId == appearanceId)?.AppearanceNote,
            };
            return appearanceDetail;
        }

        public async Task<CriminalFileContent> FilesCriminalFilecontentAsync(string agencyId, string roomCode, DateTime? proceeding, string appearanceId, string justinNumber)
        {
            var proceedingDateString = proceeding.HasValue ? proceeding.Value.ToString("yyyy-MM-dd") : "";
            return await _fileServicesClient.FilesCriminalFilecontentAsync(agencyId, roomCode,
                proceedingDateString, appearanceId, justinNumber);
        }

        public async Task<RopResponse> FilesRecordOfProceedingsAsync(string partId, string profSequenceNumber, CourtLevelCd courtLevelCode, CourtClassCd courtClassCode)
        {
            return await _fileServicesClient.FilesRecordOfProceedingsAsync(partId, profSequenceNumber, courtLevelCode, courtClassCode);
        }

        #endregion Criminal Only

        #region Courtlist & Document

        public async Task<CourtList> FilesCourtlistAsync(string agencyId, string roomCode, DateTime? proceeding, string divisionCode, string fileNumber)
        {
            var proceedingDateString = proceeding.HasValue ? proceeding.Value.ToString("yyyy-MM-dd") : "";
            return await _fileServicesClient.FilesCourtlistAsync(agencyId, roomCode, proceedingDateString, divisionCode,
                fileNumber);
        }

        public async Task<DocumentResponse> FilesDocumentAsync(string documentId, bool isCriminal)
        {
            return await _fileServicesClient.FilesDocumentAsync(documentId, isCriminal ? "R" : "I");
        }

        #endregion Courtlist & Document

        #endregion Methods

        #region Helpers

        #region Criminal Details

        private async Task<CriminalFileAppearances> PopulateCriminalDetailsAppearancesAsync(string fileId, FutureYN? future, HistoryYN? history)
        {
            var criminalFileIdAppearances = await _fileServicesClient.FilesCriminalFileIdAppearancesAsync(_requestAgencyIdentifierId, _requestPartId, future, history, fileId);
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
                //TODO COUNSEL? Not sure where  to get this data from
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

        private async Task<ICollection<Models.Criminal.AppearanceDetail.CriminalAppearanceMethod>> PopulateAppearanceMethods(ICollection<JCCommon.Clients.FileServices.CriminalAppearanceMethod> appearanceMethods)
        {
            var criminalAppearanceMethods = _mapper.Map<ICollection<Models.Criminal.AppearanceDetail.CriminalAppearanceMethod>>(appearanceMethods);
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
            var civilFileAppearancesResponse = await _fileServicesClient.FilesCivilFileIdAppearancesAsync(_requestAgencyIdentifierId, _requestPartId, future, history,
                fileId);

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

        private async Task<ICollection<CivilAppearanceDetailParty>> PopulateCivilDetailedAppearanceParties(ICollection<CivilAppearanceParty> parties)
        {
            var civilAppearanceDetailParties = _mapper.Map<ICollection<CivilAppearanceDetailParty>>(parties);
            foreach (var party in civilAppearanceDetailParties)
                party.PartyRoleTypeDesc = await _lookupService.GetCivilRoleTypeDescription(party.PartyRoleTypeCd);
            return civilAppearanceDetailParties;
        }

        private async Task<ICollection<CivilAppearanceDocument>> PopulateCivilDetailedAppearanceDocuments(List<CvfcDocument3> documentsWithSameAppearanceId)
        {
            //CivilAppearanceDocument, doesn't include appearances.
            var documents = _mapper.Map<ICollection<CivilAppearanceDocument>>(documentsWithSameAppearanceId);
            foreach (var document in documents)
            {
                document.Category = _lookupService.GetDocumentCategory(document.DocumentTypeCd);
                document.DocumentTypeDescription = await _lookupService.GetDocumentDescriptionAsync(document.DocumentTypeCd);
            }
            return documents;
        }

        #endregion Civil Appearance Details

        #endregion Helpers
    }
}