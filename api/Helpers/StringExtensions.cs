namespace Scv.Api.Helpers
{
    public static class StringExtensions
    {
        public static string EnsureEndingForwardSlash(this string target) => target.EndsWith("/") ? target : $"{target}/";
    }
}
