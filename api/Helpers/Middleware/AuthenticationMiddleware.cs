using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Scv.Api.Helpers.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private string ValidUserName { get; }
        private string ValidPassword { get; }
        private string ValidSiteMinderGuid { get; }
        private string ValidSiteMinderUserType { get; }


        public AuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            ValidUserName = configuration.GetNonEmptyValue("Auth:UserId");
            ValidPassword = configuration.GetNonEmptyValue("Auth:UserPassword");
            ValidSiteMinderGuid = configuration.GetNonEmptyValue("Auth:SCSSServiceAccountGuid");
            ValidSiteMinderUserType = configuration.GetNonEmptyValue("Auth:AllowSiteMinderUserType");
        }

        public async Task Invoke(HttpContext context)
        {

            if (context.Request.Path.Value == "/api/test/headers")
            {
                await _next.Invoke(context);
                return;
            }

            string authHeader = context.Request.Headers["Authorization"];
            string siteMinderUserGuidHeader = context.Request.Headers["SMGOV_USERGUID"];
            string siteMinderUserType = context.Request.Headers["SMGOV_USERTYPE"];

            if (siteMinderUserGuidHeader == ValidSiteMinderGuid && siteMinderUserType == ValidSiteMinderUserType)
            {
                await _next.Invoke(context);
                return;
            }

            if (authHeader != null && authHeader.StartsWith("Basic"))
            {
                var (username, password) = GetCredentialsFromBasicAuth(authHeader);
                if (ValidUserName == username && ValidPassword == password)
                {
                    await _next.Invoke(context);
                    return;
                }
            }
            context.Response.StatusCode = 401;
        }

        private (string username, string password) GetCredentialsFromBasicAuth(string authHeader)
        {
            string encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
            string usernamePassword = Encoding.GetEncoding("iso-8859-1").GetString(Convert.FromBase64String(encodedUsernamePassword));
            int seperatorIndex = usernamePassword.IndexOf(':');
            return (usernamePassword.Substring(0, seperatorIndex), usernamePassword.Substring(seperatorIndex + 1));
        }
    }
}
