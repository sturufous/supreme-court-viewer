using System;
using System.Security.Claims;
using System.Threading.Tasks;
using JCCommon.Clients.FileServices;
using LazyCache;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Serialization;
using Scv.Api.Helpers;
using Scv.Api.Helpers.ContractResolver;

namespace Scv.Api.Services.Files
{
    /// <summary>
    /// This is meant to wrap our FileServicesClient. That way we can easily extend the information provided to us by the FileServicesClient.
    /// </summary>
    public class FilesService
    {
        #region Variables

        public readonly CivilFilesService Civil;
        public readonly CriminalFilesService Criminal; 
        private readonly FileServicesClient _filesClient;
        private readonly IAppCache _cache;

        #endregion Variables

        #region Constructor

        public FilesService(IConfiguration configuration,
            FileServicesClient filesClient,
            IMapper mapper,
            LookupService lookupService,
            LocationService locationService,
            IAppCache cache,
            ClaimsPrincipal claimsPrincipal)
        {
            _filesClient = filesClient;
            _filesClient.JsonSerializerSettings.ContractResolver = new SafeContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
            _cache = cache;
            _cache.DefaultCachePolicy.DefaultCacheDurationSeconds = int.Parse(configuration.GetNonEmptyValue("Caching:FileExpiryMinutes")) * 60;
            Civil = new CivilFilesService(configuration, filesClient, mapper, lookupService, locationService, _cache, claimsPrincipal);
            Criminal = new CriminalFilesService(configuration, filesClient, mapper, lookupService, locationService, _cache, claimsPrincipal);
        }

        #endregion Constructor

        #region Methods
        
        #region Courtlist & Document

       

        public async Task<DocumentResponse> DocumentAsync(string documentId, bool isCriminal)
        {
            return await _filesClient.FilesDocumentAsync(documentId, isCriminal ? "R" : "I");
        }

        #endregion Courtlist & Document

        #endregion Methods
    }
}