using System;
using System.Collections.Generic;
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

        public static string PreferredUsername(this ClaimsPrincipal user) =>
            user.FindFirstValue(CustomClaimTypes.PreferredUsername);

        public static List<string> Groups(this ClaimsPrincipal claimsPrincipal)
        {
            var identity = (ClaimsIdentity)claimsPrincipal.Identity;
            return identity.Claims.Where(c => c.Type == CustomClaimTypes.Groups).Select(s => s.Value).ToList();
        }

        public static bool IsServiceAccountUser(this ClaimsPrincipal user)
            => user.HasClaim(c => c.Type == CustomClaimTypes.PreferredUsername) && 
               user.FindFirstValue(CustomClaimTypes.PreferredUsername).Equals("service-account-scv");

        public static bool IsIdirUser(this ClaimsPrincipal user)
        => user.HasClaim(c => c.Type == CustomClaimTypes.PreferredUsername) && 
           user.FindFirstValue(CustomClaimTypes.PreferredUsername).Contains("@idir");

        public static bool IsVcUser(this ClaimsPrincipal user)
            => user.HasClaim(c => c.Type == CustomClaimTypes.PreferredUsername) && 
               user.FindFirstValue(CustomClaimTypes.PreferredUsername).Contains("@vc");

        public static bool HasVcCivilFileAccess(this ClaimsPrincipal user, string fileId) =>
        user.HasClaim(c => c.Type == CustomClaimTypes.CivilFileAccess) && user.HasClaim(CustomClaimTypes.CivilFileAccess, fileId);
    }
}
