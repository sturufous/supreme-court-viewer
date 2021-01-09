using System;
using System.Linq;
using System.Security.Claims;

namespace Scv.Api.Helpers.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string ParticipantId(this ClaimsPrincipal claimsPrincipal)
        {
            var identity = (ClaimsIdentity)claimsPrincipal.Identity;
            return identity.Claims.FirstOrDefault(c => c.Type == CustomClaimTypes.JcParticipantId)?.Value;
        }

        public static string AgencyCode(this ClaimsPrincipal claimsPrincipal)
        {
            var identity = (ClaimsIdentity)claimsPrincipal.Identity;
            return identity.Claims.FirstOrDefault(c => c.Type == CustomClaimTypes.JcAgencyCode)?.Value;
        }
    }
}
