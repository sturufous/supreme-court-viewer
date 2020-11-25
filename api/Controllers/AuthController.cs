using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        [HttpGet("DarsRequestCivilFileAccess")]
        public async Task<IActionResult> RequestCivilFileAccess(string username, string fileId)
        {
            //DarsRequestCivilFileAccess
            return Ok("");
        }
    }
}
