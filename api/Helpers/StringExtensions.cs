namespace Scv.Api.Helpers
{
    public static class StringExtensions
    {
        public static string EnsureLeadingForwardSlash(this string target) => target.EndsWith("/") ? target : $"{target}/";
    }
}
