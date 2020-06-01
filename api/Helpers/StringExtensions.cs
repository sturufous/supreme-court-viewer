namespace Scv.Api.Helpers
{
    public static class StringExtensions
    {
        public static string EnsureEndingForwardSlash(this string target) => target.EndsWith("/") ? target : $"{target}/";
        public static string ReturnNullIfNullOrEmpty(string target) => string.IsNullOrEmpty(target) ? null : target;
    }
}
