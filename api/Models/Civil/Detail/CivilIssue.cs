using JCCommon.Clients.FileServices;
using Mapster;
using Newtonsoft.Json;
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace Scv.Api.Models.Civil.Detail
{

    public class CivilIssue : CvfcIssue2
    {
        public string IssueTypeDesc { get; set; }
        public string IssueResultCdDesc { get; set; }
    }
}