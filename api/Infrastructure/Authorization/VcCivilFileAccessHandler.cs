using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Scv.Api.Helpers.Extensions;
using Scv.Db.Models;

namespace Scv.Api.Infrastructure.Authorization
{
    public class VcCivilFileAccessHandler
    {
        private ScvDbContext Db { get; }
        public VcCivilFileAccessHandler(ScvDbContext db)
        {
            Db = db;
        }
        public async Task<Boolean> HasCivilFileAccess(ClaimsPrincipal user, string civilFileId)
        {
            var preferredUserName = user.PreferredUsername();
            var now = DateTimeOffset.Now;
            var fileAccess = await Db.RequestFileAccess
                .AnyAsync(r => r.UserId == preferredUserName && r.Expires > now && r.FileId == civilFileId);
            return fileAccess;
        }
    }
}
