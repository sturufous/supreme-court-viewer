using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Scv.Api.Infrastructure.Handler
{
    public class TimingHandler : DelegatingHandler
    {
        private readonly ILogger<TimingHandler> _logger;

        public TimingHandler(ILogger<TimingHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            var guid = Guid.NewGuid();

            _logger.LogInformation($"Starting request {guid} - {request.RequestUri.AbsoluteUri}");

            var response = await base.SendAsync(request, cancellationToken);

            _logger.LogInformation($"Finished request {guid} - {request.RequestUri.AbsoluteUri} in {sw.ElapsedMilliseconds}ms");

            return response;
        }
    }
}
