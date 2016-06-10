using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestGit
{

    public static class StringExtensionsMethods
    {
        /// <summary>
        /// Replaces the whole word.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="word">The word.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns>String.</returns>
        public static string ReplaceWholeWord(this string s, string word, string replacement)
        {
            var firstLetter = word[0];
            var sb = new System.Text.StringBuilder();
            var previousWasLetterOrDigit = false;
            var i = 0;
            while (i < s.Length - word.Length + 1)
            {
                var wordFound = false;
                var c = s[i];
                if (c == firstLetter)
                    if (!previousWasLetterOrDigit)
                        if (s.Substring(i, word.Length).Equals(word))
                        {
                            wordFound = true;
                            var wholeWordFound = true;
                            if (s.Length > i + word.Length)
                            {
                                if (char.IsLetterOrDigit(s[i + word.Length]))
                                    wholeWordFound = false;
                            }

                            sb.Append(wholeWordFound ? replacement : word);

                            i += word.Length;
                        }

                if (wordFound) continue;

                previousWasLetterOrDigit = char.IsLetterOrDigit(c);
                sb.Append(c);
                i++;
            }

            if (s.Length - i > 0)
                sb.Append(s.Substring(i));

            return sb.ToString();
        }
    }
}
