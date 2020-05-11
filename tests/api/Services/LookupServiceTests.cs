﻿using System.Diagnostics;
using JCCommon.Clients.LookupServices;
using LazyCache;
using Scv.Api.Services;
using tests.api.Helpers;
using Xunit;

namespace tests.api.Services
{
    public class LookupServiceTests
    {
        private LookupService _lookupService;
        public LookupServiceTests()
        {
            SetupLookupServiceTests();
        }

        [Fact]
        public async void Lookup_Document_Description_With_Caching()
        {
            //Call this again, so it gives us a fresh cache, because tests aren't always ran in a particular order. 
            SetupLookupServiceTests();

            var fetchTimer = Stopwatch.StartNew();
            //Should be a fetch at first.
            var lookupCode = await _lookupService.GetDocumentDescriptionAsync("RBK");
            fetchTimer.Stop();
            Assert.Equal("170 Reports for Bankruptcy", lookupCode);

            var cacheTimer = Stopwatch.StartNew();
            //Should be cached return. 
            var lookupCodeFromCache = await _lookupService.GetDocumentDescriptionAsync("RBK");
            cacheTimer.Stop();
            Assert.Equal("170 Reports for Bankruptcy", lookupCodeFromCache);
            Assert.True(fetchTimer.Elapsed > cacheTimer.Elapsed);
        }

        [Fact]
        public void Lookup_Document_Category()
        {
            var documentCategory = _lookupService.GetDocumentCategory("AAS");
            Assert.Equal("AFFIDAVITS", documentCategory);
        }

        #region Helpers
        private void SetupLookupServiceTests()
        {
            var environmentBuilder = new EnvironmentBuilder("LookupServicesClient:Username", "LookupServicesClient:Password", typeof(LookupService));
            _lookupService = new LookupService(environmentBuilder.Configuration,
                new LookupServiceClient(environmentBuilder.HttpClient), new CachingService());
        }
        #endregion
    }
}
