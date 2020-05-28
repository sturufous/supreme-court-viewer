
namespace Scv.Api.Models.Civil.Appearances
{
    public class CivilFileAppearances : JCCommon.Clients.FileServices.CivilFileAppearancesResponse
    {
        /// <summary>
        /// Extended object.
        /// </summary>
        public new System.Collections.Generic.ICollection<CivilAppearanceDetail> ApprDetail { get; set; }
    }
}
