using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Scv.Api.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// This cannot be called from AJAX or SWAGGER. It must be loaded in the browser location, because it brings the user to the SSO page. 
        /// </summary>
        /// <param name="redirectUri">URL to go back to.</param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
        [HttpGet("login")]
        public async Task<IActionResult> Login(string redirectUri = "/api")
        {
            return Redirect(redirectUri);
        }

        [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
        [HttpGet("cookie")]
        public async Task<IActionResult> Go2()
        {
            var go = GetClaimFromCookie(HttpContext, "SCV", CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(go);
        }

        private int GetClaimFromCookie(HttpContext httpContext, string cookieName, string cookieSchema)
        {
            // Get the encrypted cookie value
            var opt = httpContext.RequestServices.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
            var cookie = opt.CurrentValue.CookieManager.GetRequestCookie(httpContext, cookieName);

            // Decrypt if found
            if (!string.IsNullOrEmpty(cookie))
            {
                var dataProtector = opt.CurrentValue.DataProtectionProvider.CreateProtector("Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware", cookieSchema, "v2");

                var ticketDataFormat = new TicketDataFormat(dataProtector);
                var ticket = ticketDataFormat.Unprotect(cookie);

                var properties = ticket.Properties.Items.Values.Sum(s => s.Length) + ticket.Properties.Items.Keys.Sum(s => s.Length);

                var claimsSize = ticket.Principal.Claims.Sum(s => s.Type.Length) +
                                 ticket.Principal.Claims.Sum(s => s.Value.Length);

                return properties + claimsSize;
            }

            return -1;
        }

        [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
        [HttpGet("DarsRequestCivilFileAccess")]
        public async Task<IActionResult> RequestCivilFileAccess(string username, string fileId)
        {
            //DarsRequestCivilFileAccess
            return Ok("");
        }

    }
}
