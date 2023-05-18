using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.Core.Utilities
{
    public static class ConversionUtilities
    {
        private const string _characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Converts a number between different bases, using alpha-numeric strings
        /// </summary>
        /// <param name="toConvert"></param>
        /// <param name="fromBase"></param>
        /// <param name="toBase"></param>
        /// <returns></returns>
        public static string ConvertToBase(string toConvert, int fromBase, int toBase)
        {
            // https://stackoverflow.com/questions/53727560/generate-the-shortest-alphanumeric-save-code

            long value = 0;
            string result = "";

            foreach (char digit in toConvert.ToCharArray())
                value = (value * fromBase) + _characters.IndexOf(digit);

            while (value > 0)
            {
                result = _characters[(int)(value % toBase)] + result;
                value /= toBase;
            }

            return result;
        }

        /// <summary>
        /// Returns proper name casing of a string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetProperCase(string value)
        {
            if (value != value.ToUpper() && value != value.ToLower())
                return value.Trim();

            string[] parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < parts.Length; ++i)
            {
                if (parts[i].Length == 1)
                    parts[i] = parts[i].ToUpper();
                else if (parts[i].Length > 1)
                    parts[i] = parts[i].Substring(0, 1).ToUpper() + parts[i].Substring(1).ToLower();
            }

            return string.Join(' ', parts).Trim();
        }

    }
}
