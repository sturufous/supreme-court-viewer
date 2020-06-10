namespace Scv.Api.Helpers.Extensions
{
    public static class StringExtensions
    {
        public static string EnsureEndingForwardSlash(this string target) => target.EndsWith("/") ? target : $"{target}/";
        public static string ReturnNullIfEmpty(this string target) => string.IsNullOrEmpty(target) ? null : target;

        public static string ConvertNameLastCommaFirstToFirstLast(this string name)
        {
            var names = name?.Split(",");
            return names?.Length == 2 ? $"{names[1].Trim()} {names[0].Trim()}" : name;
        }
    }
}
