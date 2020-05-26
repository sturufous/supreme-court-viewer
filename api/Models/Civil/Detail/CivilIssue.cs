using JCCommon.Clients.FileServices;
using Mapster;
using Newtonsoft.Json;
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace Scv.Api.Models.Civil.Detail
{
    /// <summary>
    /// Originated off of CvfcIssue2, hides IssueResultCd, IssueResultDsc
    /// </summary>
    public class CivilIssue : CvfcIssue2
    {
        [JsonIgnore]
        [AdaptIgnore]
        public new string IssueResultCd { get; }

        [JsonIgnore]
        [AdaptIgnore]
        public new string IssueResultDsc { get; }
    }
}