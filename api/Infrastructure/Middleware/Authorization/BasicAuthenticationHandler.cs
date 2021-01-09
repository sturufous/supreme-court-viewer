using System;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scv.Api.Helpers;

namespace Scv.Api.Infrastructure.Middleware.Authorization
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        #region Properties
        public const string BasicAuthentication = nameof(BasicAuthentication);
        public IConfiguration Configuration { get; }
        private string ValidUserName { get; }
        private string ValidPassword { get; }
        #endregion

        #region Constructor
        public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock, 
            IConfiguration configuration) : base(options, logger, encoder, clock)
        {
            ValidUserName = configuration.GetNonEmptyValue("Auth:UserId");
            ValidPassword = configuration.GetNonEmptyValue("Auth:UserPassword");
            Configuration = configuration;
        }
        #endregion

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string authHeader = Request.Headers["Authorization"];
            if (authHeader == null || !authHeader.StartsWith("Basic"))
                return AuthenticateResult.NoResult();

            var (username, password) = GetCredentialsFromBasicAuth(authHeader);
            if (ValidUserName != username || ValidPassword != password)
                return AuthenticateResult.Fail("Invalid basic credentials.");

            var claims = new[] {
                new Claim(CustomClaimTypes.JcParticipantId,  Configuration.GetNonEmptyValue("Request:PartId")),
                new Claim(CustomClaimTypes.JcAgencyCode, Configuration.GetNonEmptyValue("Request:AgencyIdentifierId")),
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }

        #region Helpers
        private (string username, string password) GetCredentialsFromBasicAuth(string authHeader)
        {
            string encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
            string usernamePassword = Encoding.GetEncoding("iso-8859-1").GetString(Convert.FromBase64String(encodedUsernamePassword));
            int seperatorIndex = usernamePassword.IndexOf(':');
            return (usernamePassword.Substring(0, seperatorIndex), usernamePassword.Substring(seperatorIndex + 1));
        }
        #endregion
    }
}
