using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace APKToolGUI.Utils
{
    public static class StringExt
    {
        [ThreadStatic]
        private static Random threadRandom;
        
        private static Random ThreadRandom
        {
            get
            {
                if (threadRandom == null)
                    threadRandom = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId));
                return threadRandom;
            }
        }

        /// <summary>
        /// Extracts a string from the input using the provided regex pattern.
        /// </summary>
        /// <param name="pattern">The regex pattern to match.</param>
        /// <param name="input">The input string to search.</param>
        /// <returns>The matched string or empty string if no match found.</returns>
        public static string RegexExtract(string pattern, string input)
        {
            Regex regex = new Regex(pattern);
            Match matched = regex.Match(input);
            return matched.ToString();
        }

        public static string RandStr(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[ThreadRandom.Next(s.Length)]).ToArray());
        }

        public static string RandStrWithCaps(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[ThreadRandom.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Method that limits the length of text to a defined length.
        /// </summary>
        /// <param name="source">The source text.</param>
        /// <param name="maxLength">The maximum limit of the string to return.</param>
        // string limit5 = "The quick brown fox jumped over the lazy dog.".LimitLength(5);
        public static string LimitLength(this string source, int maxLength)
        {
            if (source.Length <= maxLength)
            {
                return source;
            }

            return source.Substring(0, maxLength);
        }

        public static IEnumerable<string> SplitByLength(this string str, int maxLength)
        {
            for (int index = 0; index < str.Length; index += maxLength)
            {
                yield return str.Substring(index, Math.Min(maxLength, str.Length - index));
            }
        }

        public static string RemoveLast(this string text, string character)
        {
            try
            {
                if (text.Length < 1) return text;
                return text.Remove(text.ToString().LastIndexOf(character), character.Length);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine($"[StringExt] Character not found in text: {ex.Message}");
                return text;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StringExt] Failed to remove last character: {ex.Message}");
                return text;
            }
        }

        public static bool ContainsAny(this string haystack, params string[] needles)
        {
            foreach (string needle in needles)
            {
                if (haystack.Contains(needle))
                    return true;
            }

            return false;
        }
    }
}
