using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JCCommon.Clients.FileServices;
using JCCommon.Models;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Scv.Api.Helpers;
using Scv.Api.Helpers.ContractResolver;
using Scv.Api.Helpers.Exceptions;
using Scv.Api.Models.Civil.Detail;
using Scv.Api.Models.Criminal.Content;
using Scv.Api.Models.Criminal.Detail;
using CivilAppearanceDetail = Scv.Api.Models.Civil.Detail.CivilAppearanceDetail;

namespace Scv.Api.Services
{
    /// <summary>
    /// This is meant to wrap our FileServicesClient. That way we can easily extend the information provided to us by the FileServicesClient. 
    /// </summary>
    public class FilesService
    {
        #region Variables
        private readonly FileServicesClient _fileServicesClient;
        private readonly IMapper _mapper;
        private readonly LookupService _lookupService;
        private readonly LocationService _locationService; 
        private readonly string _requestApplicationCode;
        private readonly string _requestAgencyIdentifierId;
        private readonly string _requestPartId;
        #endregion 

        #region Constructor
        public FilesService(IConfiguration configuration, FileServicesClient fileServicesClient, IMapper mapper, LookupService lookupService, LocationService locationService)
        {
            _fileServicesClient = fileServicesClient;
            _fileServicesClient.JsonSerializerSettings.ContractResolver = new SafeContractResolver();
            _fileServicesClient.BaseUrl = configuration.GetNonEmptyValue("FileServicesClient:Url");
            _lookupService = lookupService;
            _locationService = locationService;
            _mapper = mapper;
            _requestApplicationCode = configuration.GetNonEmptyValue("Request:ApplicationCd");
            _requestAgencyIdentifierId = configuration.GetNonEmptyValue("Request:AgencyIdentifierId");
            _requestPartId = configuration.GetNonEmptyValue("Request:PartId");
        }
        #endregion

        #region Methods

        #region Civil Only
        public async Task<FileSearchResponse> FilesCivilAsync(FilesCivilQuery fcq)
        {
            fcq.FilePermissions =
                "[\"A\", \"Y\", \"T\", \"F\", \"C\", \"M\", \"L\", \"R\", \"B\", \"D\", \"E\", \"G\", \"H\", \"N\", \"O\", \"P\", \"S\", \"V\"]"; // for now, use all types - TODO: determine proper list of types?
            var fileSearchResponse = await _fileServicesClient.FilesCivilAsync(_requestAgencyIdentifierId, _requestPartId,
                _requestApplicationCode, fcq.SearchMode, fcq.FileHomeAgencyId, fcq.FileNumber, fcq.FilePrefix,
                fcq.FilePermissions, fcq.FileSuffixNumber, fcq.MDocReferenceTypeCode, fcq.CourtClass, fcq.CourtLevel,
                fcq.NameSearchType, fcq.LastName, fcq.OrgName, fcq.GivenName, fcq.Birth?.ToString("yyyy-MM-dd"),
                fcq.SearchByCrownPartId, fcq.SearchByCrownActiveOnly, fcq.SearchByCrownFileDesignation,
                fcq.MdocJustinNumberSet, fcq.PhysicalFileIdSet);
            return fileSearchResponse;
        }

        public async Task<RedactedCivilFileDetailResponse> FilesCivilFileIdAsync(string fileId)
        {
            var civilFileDetailResponse = await _fileServicesClient.FilesCivilFileIdAsync(_requestAgencyIdentifierId, _requestPartId, fileId);

            //Add in CSRs. 
            foreach (var appearance in civilFileDetailResponse.Appearance)
            {
                civilFileDetailResponse.Document.Add(new CvfcDocument3
                {
                    CivilDocumentId = appearance.AppearanceId,
                    ImageId = appearance.AppearanceId,
                    DocumentTypeCd = "CSR",
                    LastAppearanceDt = appearance.AppearanceDate,
                    FiledDt = appearance.AppearanceDate
                });
            }

            var civilFileDetail = _mapper.Map<RedactedCivilFileDetailResponse>(civilFileDetailResponse);

            //Populate location information.
            civilFileDetail.HomeLocationAgencyCode = await _locationService.GetLocationAgencyIdentifier(civilFileDetail.HomeLocationAgenId);
            civilFileDetail.HomeLocationAgencyName = await _locationService.GetLocationName(civilFileDetail.HomeLocationAgenId);

            civilFileDetail.CourtClassDescription = await _lookupService.GetCourtClassDescription(civilFileDetail.CourtClassCd.ToString());
            civilFileDetail.CourtLevelDescription = await _lookupService.GetCourtLevelDescription(civilFileDetail.CourtLevelCd.ToString());
            civilFileDetail.ActivityClassCd = await _lookupService.GetActivityClassCd(civilFileDetail.CourtClassCd.ToString());

            //Populate extra fields for party. 
            foreach (var party in civilFileDetail.Party)
                party.RoleTypeDescription = await _lookupService.GetCivilRoleTypeDescription(party.RoleTypeCd);

            //Populate extra fields for document.
            foreach (var document in civilFileDetail.Document)
            {
                document.Category = _lookupService.GetDocumentCategory(document.DocumentTypeCd);
                document.DocumentTypeDescription = await _lookupService.GetDocumentDescriptionAsync(document.DocumentTypeCd);
                document.ImageId = document.DocumentTypeCd != "CSR" && document.SealedYN != "N" ? null : document.ImageId;
            }

            //TODO need permission for this filter. 
            var hearingRescriptionPermission = true;
            civilFileDetail.HearingRestriction = civilFileDetail.HearingRestriction.Where(hr =>
                hearingRescriptionPermission &&
                hr.HearingRestrictionTypeCd != CvfcHearingRestriction2HearingRestrictionTypeCd.S).ToList();
            return civilFileDetail;
        }

        public async Task<CivilFileAppearancesResponse> FilesCivilFileIdAppearancesAsync(FutureYN2? future, HistoryYN2? history, string fileId)
        {
            var civilFileAppearancesResponse = await _fileServicesClient.FilesCivilFileIdAppearancesAsync(_requestAgencyIdentifierId, _requestPartId, future, history,
                fileId);
            return civilFileAppearancesResponse;
        }

        public async Task<CivilAppearanceDetail> FilesCivilDetailedAppearance(string fileId, string appearanceId)
        {
            var detailedAppearance = new CivilAppearanceDetail {PhysicalFileId = fileId};

            var fileDetailResponse = await _fileServicesClient.FilesCivilFileIdAsync(_requestAgencyIdentifierId, _requestPartId, fileId);

            var documentsWithSameAppearanceId = fileDetailResponse.Document.Where(doc =>
                    doc.Appearance != null && doc.Appearance.Any(app => app.AppearanceId == appearanceId))
                .ToList();

            //CivilAppearanceDocument, doesn't include appearances. 
            detailedAppearance.Document = _mapper.Map<ICollection<CivilAppearanceDocument>>(documentsWithSameAppearanceId);

            foreach (var document in detailedAppearance.Document)
            {
                document.Category = _lookupService.GetDocumentCategory(document.DocumentTypeCd);
                document.DocumentTypeDescription = await _lookupService.GetDocumentDescriptionAsync(document.DocumentTypeCd);
            }

            var civilFileAppearancePartyResponse = await _fileServicesClient.FilesCivilAppearancesAppearanceIdPartyAsync(_requestAgencyIdentifierId, _requestPartId, appearanceId);
            detailedAppearance.Party = civilFileAppearancePartyResponse.Party;

            var civilFileAppearanceApprMethodResponse = await _fileServicesClient.FilesCivilAppearancesAppearanceIdAppearancemethodAsync(
                    _requestAgencyIdentifierId, _requestPartId, appearanceId);
            detailedAppearance.AppearanceMethod = civilFileAppearanceApprMethodResponse.AppearanceMethod;

            return detailedAppearance;
        }

        public async Task<JustinReportResponse> FilesCivilCourtsummaryreportAsync(string appearanceId, string reportName)
        {
            var justinReportResponse = await _fileServicesClient.FilesCivilCourtsummaryreportAsync(_requestAgencyIdentifierId,
                _requestPartId, appearanceId, reportName);
            return justinReportResponse;
        }

        public async Task<object> FilesCivilFilecontentAsync(string agencyId, string roomCode, DateTime? proceeding, string appearanceId, string physicalFileId)
        {
            var proceedingDateString = proceeding.HasValue ? proceeding.Value.ToString("yyyy-MM-dd") : "";
            var civilFileContent = await _fileServicesClient.FilesCivilFilecontentAsync(agencyId, roomCode, proceedingDateString,
                appearanceId, physicalFileId);
            return civilFileContent;
        }
        #endregion

        #region Criminal Only
        public async Task<FileSearchResponse> FilesCriminalAsync(FilesCriminalQuery fcq)
        {
            fcq.FilePermissions =
                "[\"A\", \"Y\", \"T\", \"F\", \"C\", \"M\", \"L\", \"R\", \"B\", \"D\", \"E\", \"G\", \"H\", \"N\", \"O\", \"P\", \"S\", \"V\"]"; // for now, use all types - TODO: determine proper list of types?

            //CourtLevel = "S"  Supreme court data, CourtLevel = "P" - Province.
            var fileSearchResponse = await _fileServicesClient.FilesCriminalAsync(_requestAgencyIdentifierId,
                _requestPartId, _requestApplicationCode, fcq.SearchMode, fcq.FileHomeAgencyId, fcq.FileNumberTxt,
                fcq.FilePrefixTxt, fcq.FilePermissions, fcq.FileSuffixNo, fcq.MdocRefTypeCode, fcq.CourtClass,
                fcq.CourtLevel, fcq.NameSearchTypeCd, fcq.LastName, fcq.OrgName, fcq.GivenName,
                fcq.Birth?.ToString("yyyy-MM-dd"), fcq.SearchByCrownPartId, fcq.SearchByCrownActiveOnly,
                fcq.SearchByCrownFileDesignation, fcq.MdocJustinNoSet, fcq.PhysicalFileIdSet);
            return fileSearchResponse;
        }

        public async Task<RedactedCriminalFileDetailResponse> FilesCriminalFileIdAsync(string fileId)
        {
            var criminalFileDetail = await _fileServicesClient.FilesCriminalFileIdAsync(_requestAgencyIdentifierId, _requestPartId, _requestApplicationCode, fileId);
            var criminalFileContent = await _fileServicesClient.FilesCriminalFilecontentAsync(null, null, null, null, fileId);

            //CriminalFileContent can return null when an invalid fileId is inserted. 
            if (criminalFileDetail == null || criminalFileContent == null)
                return null;

            var redactedCriminalFileDetailResponse = _mapper.Map<RedactedCriminalFileDetailResponse>(criminalFileDetail);

            //Generate documents from AccusedFile. 
            var documents = criminalFileContent.AccusedFile.SelectMany(ac =>
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
                    document.PartId = ac.PartId;
                }

                return criminalDocuments;
            }).ToList();

            //Attach documents to participants.
            foreach (var participant in redactedCriminalFileDetailResponse.Participant)
            {
                participant.Document = documents.Where(doc => doc.PartId == participant.PartId).ToList();
            }

            return redactedCriminalFileDetailResponse;
        }

        public async Task<CriminalFileAppearancesResponse> FilesCriminalFileIdAppearancesAsync(string fileId, FutureYN? future, HistoryYN? history)
        {
            var criminalFileIdAppearances = await _fileServicesClient.FilesCriminalFileIdAppearancesAsync(_requestAgencyIdentifierId, _requestPartId, future, history, fileId);
            return criminalFileIdAppearances;
        }

        public async Task<CriminalFileContent> FilesCriminalFilecontentAsync(string agencyId, string roomCode, DateTime? proceeding, string appearanceId, string justinNumber)
        {
            var proceedingDateString = proceeding.HasValue ? proceeding.Value.ToString("yyyy-MM-dd") : "";
            
            var criminalFileContent = await _fileServicesClient.FilesCriminalFilecontentAsync(agencyId, roomCode,
                proceedingDateString, appearanceId, justinNumber);

            return criminalFileContent;
        }

        public async Task<RopResponse> FilesRecordOfProceedingsAsync(string partId, string profSequenceNumber, CourtLevelCd courtLevelCode, CourtClassCd courtClassCode)
        {
            var recordsOfProceeding = await _fileServicesClient.FilesRecordOfProceedingsAsync(partId, profSequenceNumber, courtLevelCode, courtClassCode);
            return recordsOfProceeding;
        }
        #endregion

        public async Task<CourtList> FilesCourtlistAsync(string agencyId, string roomCode, DateTime? proceeding, string divisionCode, string fileNumber)
        {
            var proceedingDateString = proceeding.HasValue ? proceeding.Value.ToString("yyyy-MM-dd") : "";
            var courtList = await _fileServicesClient.FilesCourtlistAsync(agencyId, roomCode, proceedingDateString, divisionCode,
                fileNumber);
            return courtList;
        }

        public async Task<DocumentResponse> FilesDocumentAsync(string documentId, bool isCriminal)
        {
            var documentResponse = await _fileServicesClient.FilesDocumentAsync(documentId, isCriminal ? "R" : "I");
            return documentResponse;
        }
        #endregion
    }
}