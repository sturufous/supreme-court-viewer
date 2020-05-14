namespace Scv.Api.Models.Civil.Detail
{
    /// <summary>
    /// Originated off of CvfcIssue2, hides IssueResultCd, IssueResultDsc
    /// </summary>
    public class CivilIssue
    {
        [Newtonsoft.Json.JsonProperty("issueTypeCd", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string IssueTypeCd { get; set; }
        
        [Newtonsoft.Json.JsonProperty("issueNumber", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string IssueNumber { get; set; }

        [Newtonsoft.Json.JsonProperty("issueDsc", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string IssueDsc { get; set; }
        
        [Newtonsoft.Json.JsonProperty("concludedYn", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string ConcludedYn { get; set; }
    }
}
