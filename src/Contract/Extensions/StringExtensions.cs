namespace FireInvent.Contract.Extensions
{
    public static class StringExtensions
    {
        public static string SanitizeForLogging(this string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return System.Text.RegularExpressions.Regex.Replace(input, @"[\r\n\t\x00-\x1F\x7F]", "_");
        }
    }
}
