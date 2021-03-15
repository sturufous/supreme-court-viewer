using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Scv.Api.Helpers;
using Scv.Api.Helpers.Extensions;
using Scv.Api.Infrastructure.Authorization;
using Scv.Api.Models.auth;
using Scv.Db.Models;
using Scv.Db.Models.Auth;

namespace Scv.Api.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        public ScvDbContext Db { get; }
        public IConfiguration Configuration { get; }

        public AuthController(ScvDbContext db, IConfiguration configuration)
        {
            Db = db;
            Configuration = configuration;
        }
        /// <summary>
        /// This cannot be called from AJAX or SWAGGER. It must be loaded in the browser location, because it brings the user to the SSO page. 
        /// </summary>
        /// <param name="redirectUri">URL to go back to.</param>
        [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
        [HttpGet("login")]
        public IActionResult Login(string redirectUri = "/api")
        {
            return Redirect(redirectUri);
        }

        /// <summary>
        /// Logout function, should wipe out all cookies. 
        /// </summary>
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

            var logoutUrl = $"{Configuration.GetNonEmptyValue("Keycloak:Authority")}/protocol/openid-connect/logout";

            var forwardedHost = HttpContext.Request.Headers.ContainsKey("X-Forwarded-Host")
                ? HttpContext.Request.Headers["X-Forwarded-Host"].ToString()
                : Request.Host.ToString();
            var forwardedPort = HttpContext.Request.Headers["X-Forwarded-Port"];

            //We are always sending X-Forwarded-Port, only time we aren't is when we are hitting the API directly. 
            var baseUri = HttpContext.Request.Headers.ContainsKey("X-Forwarded-Host") ? $"{Configuration.GetNonEmptyValue("WebBaseHref")}logout" : "/api";

            var applicationUrl = $"{XForwardedForHelper.BuildUrlString(forwardedHost, forwardedPort, baseUri)}";
            var keycloakLogoutUrl = $"{logoutUrl}?post_logout_redirect_uri={applicationUrl}";
            var siteMinderLogoutUrl = $"{Configuration.GetNonEmptyValue("SiteMinderLogoutUrl")}?returl={keycloakLogoutUrl}&retnow=1";
            return Redirect(siteMinderLogoutUrl);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("request-civil-file-access")]
        public async Task<IActionResult> RequestCivilFileAccess([FromBody] RequestCivilFileAccess request)
        {
            if (string.IsNullOrEmpty(request.FileId) || string.IsNullOrEmpty(request.UserId))
                return BadRequest();

            if (!User.IsServiceAccountUser())
                return Forbid();

            var expiryMinutes = float.Parse(Configuration.GetNonEmptyValue("RequestCivilFileAccessMinutes"));
            await Db.RequestFileAccess.AddAsync(new RequestFileAccess
            {
                FileId = request.FileId,
                UserId = request.UserId,
                Requested = DateTimeOffset.Now,
                Expires = DateTimeOffset.Now.AddMinutes(expiryMinutes)
            });
            await Db.SaveChangesAsync();

            var forwardedHost = Request.Headers["X-Forwarded-Host"];
            var forwardedPort = Request.Headers["X-Forwarded-Port"];
            var baseUrl = Configuration.GetNonEmptyValue("WebBaseHref");

            return Ok(new
            {
                Url = $"{XForwardedForHelper.BuildUrlString(forwardedHost, forwardedPort, baseUrl)}civil-file/{request.FileId}?fromA2A=true"
            });
        }

        [Authorize(AuthenticationSchemes = "SiteMinder, OpenIdConnect", Policy = nameof(ProviderAuthorizationHandler))]
        [HttpGet("info")]
        public ActionResult UserInfo()
        {
            return Ok(new
            {
                AgencyId = User.AgencyCode(),
                ParticipantId = User.ParticipantId()
            });
        }
    }
}
