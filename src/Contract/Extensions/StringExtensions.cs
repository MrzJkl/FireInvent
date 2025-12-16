using System.Text;

namespace FireInvent.Contract.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveNonAsciiCharacters(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var sb = new StringBuilder(input.Length);

            foreach (char c in input)
            {
                if (c < 128)
                    sb.Append(c);
            }

            return sb.ToString();
        }
    }
}
