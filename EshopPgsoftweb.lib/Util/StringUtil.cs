using System.Globalization;
using System.Text;

namespace eshoppgsoftweb.lib.Util
{
    public class StringUtil
    {
        /// <summary>
        /// Removes diacritics from input string
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns>Output string without diacritics</returns>
        public static string RemoveDiacritics(string input)
        {
            string normalizedString = input.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        }
    }
}
