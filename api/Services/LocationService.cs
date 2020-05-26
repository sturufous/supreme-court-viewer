using JCCommon.Clients.LocationServices;
using LazyCache;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Serialization;
using Scv.Api.Helpers;
using Scv.Api.Helpers.ContractResolver;
using System;
using System.Linq;
using System.Threading.Tasks;
using CodeValue = System.Collections.Generic.ICollection<JCCommon.Clients.LocationServices.CodeValue>;

namespace Scv.Api.Services
{
    /// <summary>
    /// This should handle caching and LocationServicesClient.
    /// </summary>
    public class LocationService
    {
        #region Variables

        private readonly IAppCache _cache;
        private readonly IConfiguration _configuration;
        private readonly LocationServicesClient _locationClient;

        #endregion Variables

        #region Properties

        private DateTimeOffset CacheExpiry => DateTimeOffset.Now.AddHours(1);

        #endregion Properties

        #region Constructor

        public LocationService(IConfiguration configuration, LocationServicesClient locationServicesClient,
            IAppCache cache)
        {
            _configuration = configuration;
            _locationClient = locationServicesClient;
            _cache = cache;
            SetupLocationServicesClient();
        }

        #endregion Constructor

        #region Collection Methods

        public async Task<CodeValue> GetLocationsFromLazyCache() => await GetDataFromCache("Locations", async () => await _locationClient.LocationsAsync(null, true, true));

        #endregion Collection Methods

        #region Lookup Methods

        public async Task<string> GetLocationName(string code) => FindLongDescriptionFromCode(await GetLocationsFromLazyCache(), code);

        public async Task<string> GetLocationAgencyIdentifier(string code) => FindShortDescriptionFromCode(await GetLocationsFromLazyCache(), code);

        public async Task<string> GetRegionName(string code) => string.IsNullOrEmpty(code) ? null : (await _locationClient.LocationsLocationIdRegionAsync(code))?.RegionName;

        #endregion Lookup Methods

        #region Helpers

        private async Task<T> GetDataFromCache<T>(string key, Func<Task<T>> fetchFunction)
        {
            return await _cache.GetOrAddAsync(key,
                async () => await fetchFunction.Invoke(), CacheExpiry);
        }

        private string FindLongDescriptionFromCode(CodeValue lookupCodes, string code) => lookupCodes.FirstOrDefault(lookupCode => lookupCode.Code == code)?.LongDesc;

        private string FindShortDescriptionFromCode(CodeValue lookupCodes, string code) => lookupCodes.FirstOrDefault(lookupCode => lookupCode.Code == code)?.ShortDesc;

        private void SetupLocationServicesClient()
        {
            _locationClient.JsonSerializerSettings.ContractResolver = new SafeContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
            _locationClient.BaseUrl = _configuration.GetNonEmptyValue("LocationServicesClient:Url");
        }

        #endregion Helpers
    }
}