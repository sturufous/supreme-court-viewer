using JCCommon.Clients.FileServices;

namespace Scv.Api.Models.Civil.AppearanceDetail
{
    public class CivilCounsel : ClCounsel
    {
        public string CounselAppearanceMethod { get; set; }
        public string CounselAppearanceMethodDesc { get; set; }
    }
}
