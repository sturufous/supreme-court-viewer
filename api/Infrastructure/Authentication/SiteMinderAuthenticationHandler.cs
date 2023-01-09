using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scv.Api.Helpers;
using Scv.Api.Helpers.Extensions;
using Scv.Api.Models.JCUserService;
using Scv.Api.Services;

namespace Scv.Api.Infrastructure.Authentication
{
    public class SiteMinderAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        #region Properties 
        public const string SiteMinder = nameof(SiteMinder);
        private JCUserService JCUserService { get; }
        private string ValidSiteMinderUserType { get; }
        #endregion

        #region Constructor
        public SiteMinderAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration configuration, JCUserService jcUserService) : base(options, logger, encoder, clock)
        {
            JCUserService = jcUserService;
            ValidSiteMinderUserType = configuration.GetNonEmptyValue("Auth:AllowSiteMinderUserType");
        }
        #endregion

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            Logger.LogInformation("Authenticating with SiteMinder");
            string siteMinderUserGuidHeader = Request.Headers["SMGOV_USERGUID"];
            string siteMinderUserTypeHeader = Request.Headers["SMGOV_USERTYPE"];
            Logger.LogInformation("SMGOV_USERGUID: {0}", siteMinderUserGuidHeader);
            Logger.LogInformation("SMGOV_USERTYPE: {0}", siteMinderUserTypeHeader);

            if (siteMinderUserGuidHeader == null || siteMinderUserTypeHeader == null)
            {
                Logger.LogInformation("One of the headers was null");
                return AuthenticateResult.NoResult();
            }

            if (siteMinderUserTypeHeader != ValidSiteMinderUserType)
            {
                Logger.LogInformation("USERTYPE does not match ValidSiteMinderUserType: {0} vs {1}", siteMinderUserTypeHeader, ValidSiteMinderUserType);
                return AuthenticateResult.Fail("Invalid SiteMinder UserType Header.");
            }

            // set authtype when we get guid, find out where this context is being set for identity
            Logger.LogInformation("Context.User.Identity all:");
            Logger.LogInformation("\tContext.User.Identity.AuthenticationType: {0}", Context.User.Identity.AuthenticationType);
            Logger.LogInformation("\tContext.User.Identity.IsAuthenticated: {0}", Context.User.Identity.IsAuthenticated);
            Logger.LogInformation("\tContext.User.Identity.Name: {0}", Context.User.Identity.Name);

            var authenticatedBySiteMinderPreviously = Context.User.Identity.AuthenticationType == SiteMinder;
            var applicationCode = Context.User.ApplicationCode();
            var participantId = Context.User.ParticipantId();
            var agencyCode = Context.User.AgencyCode();
            var isSupremeUser = Context.User.IsSupremeUser();
            var role = Context.User.Role();
            var subRole = Context.User.SubRole();

            Logger.LogInformation("SiteMinder: {0}", SiteMinder);
            Logger.LogInformation("authenticatedBySiteMinderPreviously: {0}", authenticatedBySiteMinderPreviously);
            Logger.LogInformation("applicationCode: {0}", applicationCode);
            Logger.LogInformation("participantId : {0}", participantId);
            Logger.LogInformation("agencyCode : {0}", agencyCode);
            Logger.LogInformation("isSupremeUser : {0}", isSupremeUser);
            Logger.LogInformation("role : {0}", role);
            Logger.LogInformation("subRole : {0}", subRole);

            if (!authenticatedBySiteMinderPreviously || role == null || subRole == null)
            {
                Logger.LogInformation("Not Authenticated through siteminder previously, checking against JCI");
                var request = new UserInfoRequest
                {
                    DeviceName = Environment.MachineName,
                    DomainUserGuid = siteMinderUserGuidHeader,
                    DomainUserId = Request.Headers["SM_USER"],
                    IpAddress = Request.Headers["X-Real-IP"],
                    TemporaryAccessGuid = ""
                };
                var jcUserInfo = await JCUserService.GetUserInfo(request);

                if (jcUserInfo == null)
                {
                    Logger.LogInformation("JCUserService Response == null");
                    return AuthenticateResult.Fail("Couldn't authenticate through JC-Interface.");
                }

                applicationCode = "SCV";
                participantId = jcUserInfo.PartID;
                agencyCode = jcUserInfo.AgenID;
                role = jcUserInfo.RoleCd;
                subRole = jcUserInfo.SubRoleCd;
                isSupremeUser = true;
            }

            var claims = new[] {
                new Claim(CustomClaimTypes.ApplicationCode, applicationCode),
                new Claim(CustomClaimTypes.JcParticipantId, participantId),
                new Claim(CustomClaimTypes.JcAgencyCode, agencyCode),
                new Claim(CustomClaimTypes.Role, role),
                new Claim(CustomClaimTypes.SubRole, subRole),
                new Claim(CustomClaimTypes.IsSupremeUser, isSupremeUser.ToString()),
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);

            if (!authenticatedBySiteMinderPreviously)
            {
                Logger.LogInformation("Sign in with principal if not authenticated previously");
                await Context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }
                
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            Logger.LogInformation("Scheme.Name: {0}", Scheme.Name);
            Logger.LogInformation("Successfully logged in");
            return AuthenticateResult.Success(ticket);
        }
    }
}
