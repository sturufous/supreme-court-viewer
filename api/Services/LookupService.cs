using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JCCommon.Clients.LookupServices;
using LazyCache;
using Microsoft.Extensions.Configuration;
using Scv.Api.Helpers;
using Scv.Api.Helpers.ContractResolver;
using Scv.Api.Helpers.Exceptions;
using CodeLookup = System.Collections.Generic.ICollection<JCCommon.Clients.LookupServices.LookupCode>;

namespace Scv.Api.Services
{
    /// <summary>
    /// This should handle caching and LookupServicesClient.
    /// </summary>
    public class LookupService
    {
        #region Variables
        private readonly IAppCache _cache;
        private readonly IConfiguration _configuration;
        private readonly LookupServiceClient _lookupClient;
        #endregion

        #region Properties
        private DateTimeOffset CacheExpiry => DateTimeOffset.Now.AddHours(1);
        #endregion

        #region Constructor
        public LookupService(IConfiguration configuration, LookupServiceClient lookupClient, IAppCache cache)
        {
            _configuration = configuration;
            _lookupClient = lookupClient;
            _cache = cache;
            SetupLookupServicesClient();
        }
        #endregion

        #region LazyCache
        public async Task<CodeLookup> GetCourtClassDescriptionsFromLazyCache() => await _cache.GetOrAddAsync("CourtClassDescriptions", async () => await _lookupClient.CodesCourtClassesAsync(), CacheExpiry);
        public async Task<CodeLookup> GetCourtLevelDescriptionsFromLazyCache() => await _cache.GetOrAddAsync("CourtLevelDescriptions", async () => await _lookupClient.CodesCourtLevelsAsync(), CacheExpiry);
        /// <summary>
        ///  Uses LazyCache to query LookupServiceClient for lookup codes. This is about 600 codes atm.
        /// </summary>
        /// <returns>Task{CodeLookup}</returns>
        public async Task<CodeLookup> GetDocumentCodesFromLazyCache() => await _cache.GetOrAddAsync("LookupDocumentDescriptions", async () => await _lookupClient.CodesDocumentsAsync(), CacheExpiry);
        public async Task<CodeLookup> GetRoleTypeCodesFromLazyCache() => await _cache.GetOrAddAsync("LookupRoleTypeDescriptions", async () => await _lookupClient.CodesRolesAsync(), CacheExpiry);
        #endregion

        #region Lookup Methods
        public async Task<string> GetActivityClassCd(string code) => FindLongDescriptionFromCode(await GetCourtClassDescriptionsFromLazyCache(), code);
        public async Task<string> GetCourtClassDescription(string code) => FindShortDescriptionFromCode(await GetCourtClassDescriptionsFromLazyCache(), code);
        public async Task<string> GetCourtLevelDescription(string code) => FindShortDescriptionFromCode(await GetCourtLevelDescriptionsFromLazyCache(), code);
        public async Task<string> GetCivilRoleTypeDescription(string code) => FindShortDescriptionFromCode(await GetRoleTypeCodesFromLazyCache(), code);
        public async Task<string> GetDocumentDescriptionAsync(string code) => FindShortDescriptionFromCode(await GetDocumentCodesFromLazyCache(), code);

        /// <summary>
        /// Reads from the configuration for the document category.
        /// </summary>
        /// <param name="documentCode"></param>
        /// <returns>string</returns>
        public string GetDocumentCategory(string documentCode)
        {
            var configurationSections =
                _configuration.GetSection("DocumentCategories").Get<Dictionary<string, string>>() ??
                throw new ConfigurationException("Couldn't not build dictionary based on DocumentCategories");
            return configurationSections.FirstOrDefault(cs => cs.Value.Split(",").Contains(documentCode)).Key ?? "";
        }
        #endregion

        #region Helpers
        private string FindShortDescriptionFromCode(CodeLookup lookupCodes, string code) => lookupCodes.FirstOrDefault(lookupCode => lookupCode.Code == code)?.ShortDesc ?? "";
        private string FindLongDescriptionFromCode(CodeLookup lookupCodes, string code) => lookupCodes.FirstOrDefault(lookupCode => lookupCode.Code == code)?.LongDesc ?? "";

        private void SetupLookupServicesClient()
        {
            _lookupClient.JsonSerializerSettings.ContractResolver = new SafeContractResolver();
            _lookupClient.BaseUrl = _configuration.GetNonEmptyValue("LookupServicesClient:Url");
        }
        #endregion
    }
}