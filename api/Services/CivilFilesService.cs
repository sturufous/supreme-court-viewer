using JCCommon.Clients.FileServices;
using JCCommon.Models;
using LazyCache;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Serialization;
using Scv.Api.Helpers;
using Scv.Api.Helpers.ContractResolver;
using Scv.Api.Helpers.Extensions;
using Scv.Api.Models.Civil.AppearanceDetail;
using Scv.Api.Models.Civil.Appearances;
using Scv.Api.Models.Civil.Detail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CivilAppearanceDetail = Scv.Api.Models.Civil.AppearanceDetail.CivilAppearanceDetail;
using CivilAppearanceMethod = Scv.Api.Models.Civil.AppearanceDetail.CivilAppearanceMethod;

namespace Scv.Api.Services
{
    public class CivilFilesService
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

        public CivilFilesService(IConfiguration configuration, FileServicesClient filesClient, IMapper mapper, LookupService lookupService, LocationService locationService, IAppCache cache)
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

        public async Task<FileSearchResponse> SearchAsync(FilesCivilQuery fcq)
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

        public async Task<RedactedCivilFileDetailResponse> FileIdAsync(string fileId)
        {
            async Task<CivilFileDetailResponse> FileDetails() => await _filesClient.FilesCivilFileIdAsync(_requestAgencyIdentifierId, _requestPartId, fileId);
            async Task<CivilFileContent> FileContent() => await _filesClient.FilesCivilFilecontentAsync(null, null, null, null, fileId);
            async Task<CivilFileAppearancesResponse> Appearances() => await PopulateDetailAppearancesAsync(FutureYN2.Y, HistoryYN2.Y, fileId);

            var fileDetailTask = _cache.GetOrAddAsync($"CivilFileDetail-{fileId}", FileDetails);
            var fileContentTask = _cache.GetOrAddAsync($"CivilFileContent-{fileId}", FileContent);
            var appearancesTask = _cache.GetOrAddAsync($"CivilAppearancesFull-{fileId}", Appearances);

            var fileDetail = await fileDetailTask;
            var appearances = await appearancesTask;
            var fileContent = await fileContentTask;

            if (fileDetail?.PhysicalFileId == null)
                return null;

            var detail = _mapper.Map<RedactedCivilFileDetailResponse>(fileDetail);
            foreach (var document in PopulateDetailCsrsDocuments(fileDetail.Appearance))
                detail.Document.Add(document);

            detail = await PopulateBaseDetail(detail);
            detail.Appearances = appearances;
            detail.FileCommentText = fileContent.CivilFile.First(cf => cf.PhysicalFileID == fileId).FileCommentText;
            detail.Party = await PopulateDetailParties(detail.Party);
            detail.Document = await PopulateDetailDocuments(detail.Document);
            detail.HearingRestriction = await PopulateDetailHearingRestrictions(fileDetail.HearingRestriction);
            return detail;
        }

        public async Task<CivilAppearanceDetail> DetailedAppearanceAsync(string fileId, string appearanceId)
        {
            async Task<CivilFileDetailResponse> FileDetails() => await _filesClient.FilesCivilFileIdAsync(_requestAgencyIdentifierId, _requestPartId, fileId);
            async Task<CivilFileContent> FileContent() => await _filesClient.FilesCivilFilecontentAsync(null, null, null, null, fileId);
            async Task<CivilFileAppearancePartyResponse> AppearanceParty() => await _filesClient.FilesCivilAppearanceAppearanceIdPartiesAsync(_requestAgencyIdentifierId, _requestPartId, appearanceId);
            async Task<CivilFileAppearanceApprMethodResponse> AppearanceMethods() => await _filesClient.FilesCivilAppearanceAppearanceIdAppearancemethodsAsync(_requestAgencyIdentifierId, _requestPartId, appearanceId);
            async Task<CivilFileAppearancesResponse> Appearances() => await PopulateDetailAppearancesAsync(FutureYN2.Y, HistoryYN2.Y, fileId);

            var fileDetailTask = _cache.GetOrAddAsync($"CivilFileDetail-{fileId}", FileDetails);
            var appearancePartyTask = _cache.GetOrAddAsync($"CivilAppearanceParty-{fileId}-{appearanceId}", AppearanceParty);
            var fileContentTask = _cache.GetOrAddAsync($"CivilFileContent-{fileId}", FileContent);
            var appearanceMethodsTask = _cache.GetOrAddAsync($"CivilAppearanceMethods-{fileId}-{appearanceId}", AppearanceMethods);
            var appearancesTask = _cache.GetOrAddAsync($"CivilAppearancesFull-{fileId}", Appearances);

            var detail = await fileDetailTask;
            var appearances = await appearancesTask;
            var agencyId = await _locationService.GetLocationAgencyIdentifier(detail?.HomeLocationAgenId);
            var fileContent = await fileContentTask;

            var targetAppearance = appearances?.ApprDetail?.FirstOrDefault(app => app.AppearanceId == appearanceId);
            if (targetAppearance == null || detail == null)
                return null;

            //Sometimes we can have a bogus location, querying court list wont work.
            ClCivilCourtList civilCourtList = null;
            if (agencyId != null)
            {
                async Task<CourtList> CourtList() => await _filesClient.FilesCourtlistAsync(agencyId, targetAppearance.CourtRoomCd, targetAppearance.AppearanceDt, "CI", detail.FileNumberTxt);
                var courtListTask = _cache.GetOrAddAsync($"CivilCourtList-{agencyId}-{targetAppearance.CourtRoomCd}-{targetAppearance.AppearanceDt}-{detail.FileNumberTxt}", CourtList);
                var courtList = await courtListTask;
                civilCourtList = courtList.CivilCourtList.FirstOrDefault(cl => cl.AppearanceId == appearanceId);
            }

            var appearanceParty = await appearancePartyTask;
            var appearanceMethods = await appearanceMethodsTask;

            var appearanceDetail = appearances.ApprDetail?.FirstOrDefault(app => app.AppearanceId == appearanceId);
            var fileDetailDocuments = detail.Document.Where(doc => doc.Appearance != null && doc.Appearance.Any(app => app.AppearanceId == appearanceId)).ToList();
            var previousAppearance = fileContent?.CivilFile.FirstOrDefault(cf => cf.PhysicalFileID == fileId)?.PreviousAppearance.FirstOrDefault(pa => pa?.AppearanceId == appearanceId);

            var detailedAppearance = new CivilAppearanceDetail
            {
                PhysicalFileId = fileId,
                AgencyId = await _locationService.GetLocationAgencyIdentifier(detail.HomeLocationAgenId),
                AppearanceId = appearanceId,
                AppearanceResultCd = appearanceDetail?.AppearanceResultCd,
                AppearanceReasonCd = appearanceDetail?.AppearanceReasonCd,
                AppearanceReasonDesc = await _lookupService.GetCivilAppearanceReasonsDescription(appearanceDetail?.AppearanceReasonCd),
                AppearanceResultDesc = await _lookupService.GetCivilAppearanceResultsDescription(appearanceDetail?.AppearanceResultCd),
                CourtRoomCd = targetAppearance.CourtRoomCd,
                FileNumberTxt = detail.FileNumberTxt,
                AppearanceDt = targetAppearance.AppearanceDt,
                AppearanceMethod = await PopulateAppearanceMethods(appearanceMethods.AppearanceMethod),
                Party = await PopulateDetailedAppearancePartiesAsync(appearanceParty.Party, civilCourtList?.Parties, previousAppearance, appearanceMethods.AppearanceMethod),
                Document = await PopulateDetailedAppearanceDocuments(fileDetailDocuments),
                PreviousAppearance = previousAppearance,
                Adjudicator = await PopulateDetailedAppearanceAdjudicator(previousAppearance),
                AdjudicatorComment = previousAppearance?.AdjudicatorComment
            };
            return detailedAppearance;
        }

        public async Task<JustinReportResponse> CourtSummaryReportAsync(string appearanceId, string reportName)
        {
            var justinReportResponse = await _filesClient.FilesCivilCourtsummaryreportAsync(_requestAgencyIdentifierId,
                _requestPartId, appearanceId, reportName);
            return justinReportResponse;
        }

        public async Task<CivilFileContent> FileContentAsync(string agencyId, string roomCode, DateTime? proceeding, string appearanceId, string physicalFileId)
        {
            var proceedingDateString = proceeding.HasValue ? proceeding.Value.ToString("yyyy-MM-dd") : "";
            return await _filesClient.FilesCivilFilecontentAsync(agencyId, roomCode, proceedingDateString,
                appearanceId, physicalFileId);
        }

        #endregion Methods

        #region Helpers

        #region Civil Details

        private async Task<CivilFileAppearancesResponse> PopulateDetailAppearancesAsync(FutureYN2? future, HistoryYN2? history, string fileId)
        {
            var civilFileAppearancesResponse = await _filesClient.FilesCivilFileIdAppearancesAsync(_requestAgencyIdentifierId, _requestPartId, future, history,
                fileId);
            if (civilFileAppearancesResponse == null)
                return null;

            var civilAppearances = _mapper.Map<CivilAppearanceResponse>(civilFileAppearancesResponse);
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

        private IEnumerable<CivilDocument> PopulateDetailCsrsDocuments(ICollection<CvfcAppearance> appearances)
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

        private async Task<RedactedCivilFileDetailResponse> PopulateBaseDetail(RedactedCivilFileDetailResponse detail)
        {
            detail.HomeLocationAgencyCode = await _locationService.GetLocationAgencyIdentifier(detail.HomeLocationAgenId);
            detail.HomeLocationAgencyName = await _locationService.GetLocationName(detail.HomeLocationAgenId);
            detail.HomeLocationRegionName = await _locationService.GetRegionName(detail.HomeLocationAgencyCode);
            detail.CourtClassDescription = await _lookupService.GetCourtClassDescription(detail.CourtClassCd.ToString());
            detail.CourtLevelDescription = await _lookupService.GetCourtLevelDescription(detail.CourtLevelCd.ToString());
            detail.ActivityClassCd = await _lookupService.GetActivityClassCd(detail.CourtClassCd.ToString());
            return detail;
        }

        private async Task<ICollection<CivilDocument>> PopulateDetailDocuments(ICollection<CivilDocument> documents)
        {
            //TODO permission for documents.
            //Populate extra fields for document.
            foreach (var document in documents.Where(doc => doc.Category != "CSR"))
            {
                document.Category = _lookupService.GetDocumentCategory(document.DocumentTypeCd);
                document.DocumentTypeDescription = await _lookupService.GetDocumentDescriptionAsync(document.DocumentTypeCd);
                document.ImageId = document.SealedYN != "N" ? null : document.ImageId;
                document.Appearance = null;
                foreach (var issue in document.Issue)
                {
                    issue.IssueTypeDesc = await _lookupService.GetCivilDocumentIssueType(issue.IssueTypeCd);
                }
            }
            return documents;
        }

        private async Task<ICollection<CivilParty>> PopulateDetailParties(ICollection<CivilParty> parties)
        {
            //Populate extra fields for party.
            foreach (var party in parties)
                party.RoleTypeDescription = await _lookupService.GetCivilRoleTypeDescription(party.RoleTypeCd);
            return parties;
        }

        private async Task<ICollection<CivilHearingRestriction>> PopulateDetailHearingRestrictions(ICollection<CvfcHearingRestriction2> hearingRestrictions)
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

        private async Task<ICollection<CivilAppearanceMethod>> PopulateAppearanceMethods(ICollection<JCCommon.Clients.FileServices.CivilAppearanceMethod> baseAppearanceMethods)
        {
            var appearanceMethods = _mapper.Map<ICollection<CivilAppearanceMethod>>(baseAppearanceMethods);
            foreach (var appearanceMethod in appearanceMethods)
            {
                appearanceMethod.AppearanceMethodDesc = await _lookupService.GetCivilAssetsDescription(appearanceMethod.AppearanceMethodCd);
                appearanceMethod.RoleTypeDesc = await _lookupService.GetCivilRoleTypeDescription(appearanceMethod.RoleTypeCd);
            }

            return appearanceMethods;
        }

        private async Task<CivilAdjudicator> PopulateDetailedAppearanceAdjudicator(CvfcPreviousAppearance previousAppearance)
        {
            if (previousAppearance == null)
                return null;

            return new CivilAdjudicator
            {
                FullName = previousAppearance.AdjudicatorName,
                AppearanceMethodCd = previousAppearance.AdjudicatorAppearanceMethod,
                //For Civil files, CCD uses these codes: JUSTIN:ADJ_APP_METHOD
                AppearanceMethodDesc = await _lookupService.GetCriminalAdjudicatorAttend(previousAppearance.AdjudicatorAppearanceMethod),
            };
        }

        /// <summary>
        /// This is mostly based off of getAppearanceCivilParty and expands by court list and FileContent.
        /// </summary>
        private async Task<ICollection<CivilAppearanceDetailParty>> PopulateDetailedAppearancePartiesAsync(ICollection<CivilAppearanceParty> parties,
            ICollection<ClParty> courtListParties, CvfcPreviousAppearance previousAppearance,
            ICollection<JCCommon.Clients.FileServices.CivilAppearanceMethod> civilAppearanceMethods)
        {
            var resultParties = new List<CivilAppearanceDetailParty>();
            foreach (var partyGroup in parties.GroupBy(a => a.PartyId))
            {
                //Map over our primary values from party group.
                var party = _mapper.Map<CivilAppearanceDetailParty>(partyGroup.First());

                //Get our roles from getAppearanceCivilParty. These should essentially be the same.
                party.PartyRole = await partyGroup.Select(async pg => new ClPartyRole
                {
                    RoleTypeCd = pg.PartyRoleTypeCd,
                    RoleTypeDsc = await _lookupService.GetCivilRoleTypeDescription(pg.PartyRoleTypeCd)
                }).WhenAll();

                //Get information from appearanceMethods.
                if (civilAppearanceMethods.Any(am => party.PartyRole.Any(pr => pr.RoleTypeCd == am.RoleTypeCd)))
                {
                    party.AppearanceMethodDesc = await _lookupService.GetCivilAssetsDescription(civilAppearanceMethods.First().AppearanceMethodCd);
                }

                //Get the additional information from court list.
                var courtListParty = courtListParties?.FirstOrDefault(clp => clp.PartyId == partyGroup.Key);
                if (courtListParty != null)
                {
                    party.AttendanceMethodCd = courtListParty.AttendanceMethodCd;
                    party.AttendanceMethodDesc = await _lookupService.GetCivilAssetsDescription(party.AttendanceMethodCd);
                    party.Counsel = _mapper.Map<ICollection<CivilCounsel>>(courtListParty.Counsel);
                    party.Representative = courtListParty.Representative;
                    party.LegalRepresentative = courtListParty.LegalRepresentative;
                }

                //Add additional information from previous appearance - this comes from FileContent.
                if (previousAppearance?.CourtParticipant != null && previousAppearance.CourtParticipant.Any(cp => cp.PartId == party.PartyId))
                {
                    //Can be multiple rows here, but I think they repeat for the partyRole.
                    var targetParticipant = previousAppearance.CourtParticipant.First(cp => cp.PartId == party.PartyId);
                    party.PartyAppearanceMethod = targetParticipant.PartyAppearanceMethod;
                    party.PartyAppearanceMethodDesc = await _lookupService.GetCivilPartyAttendanceType(targetParticipant.PartyAppearanceMethod);

                    //Update the counsel with their appearanceMethod.
                    foreach (var counsel in targetParticipant.Counsel.Where(coun => coun.FullNm != null || coun.CounselId != null))
                    {
                        //TODO: Not seeing any data for CounselId in DEV.
                        //Matching on name doesn't seem like a good idea.
                        var targetCounsel = party.Counsel?.FirstOrDefault(c => c.CounselId == counsel.CounselId);
                        if (targetCounsel == null)
                            continue;

                        //I think one of the definitions for cvfcCounsel inside of the RAML is bad for this. I'd change it but not sure the impact on other systems.
                        targetCounsel.CounselAppearanceMethod = counsel.AdditionalProperties.ContainsKey("counselAppearanceMethod") ? counsel.AdditionalProperties["counselAppearanceMethod"].ToString() : null;
                        targetCounsel.CounselAppearanceMethodDesc = await _lookupService.GetCivilCounselAttendanceType(targetCounsel.CounselAppearanceMethod);
                    }
                }

                resultParties.Add(party);
            }
            return resultParties;
        }

        private async Task<ICollection<CivilAppearanceDocument>> PopulateDetailedAppearanceDocuments(List<CvfcDocument3> fileDetailDocuments)
        {
            //CivilAppearanceDocument, doesn't include appearances.
            var documents = _mapper.Map<ICollection<CivilAppearanceDocument>>(fileDetailDocuments);
            foreach (var document in documents)
            {
                document.Category = _lookupService.GetDocumentCategory(document.DocumentTypeCd);
                document.DocumentTypeDescription = await _lookupService.GetDocumentDescriptionAsync(document.DocumentTypeCd);

                foreach (var issue in document.Issue)
                {
                    issue.IssueTypeDesc = await _lookupService.GetCivilDocumentIssueType(issue.IssueTypeCd);
                }
            }
            return documents;
        }

        #endregion Civil Appearance Details

        #endregion Helpers
    }
}