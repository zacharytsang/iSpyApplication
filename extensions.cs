using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;

namespace iSpyApplication
{
    public static class Extensions
    {
        public static string ReplaceString(this string str, string oldValue, string newValue)
        {
            var sb = new StringBuilder();

            int previousIndex = 0;
            int index = str.IndexOf(oldValue, StringComparison.CurrentCultureIgnoreCase);
            while (index != -1)
            {
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, StringComparison.CurrentCultureIgnoreCase);
            }
            sb.Append(str.Substring(previousIndex));

            return sb.ToString();
        }

        public static bool IsValidEmail(this string email)
        {
            const string strRegex = @"^([a-zA-Z0-9_\-\.\+]+)@((\[[0-9]{1,3}" +
                                    @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                                    @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            var re = new Regex(strRegex);
            return re.IsMatch(email);
        }

        public static Color ToColor(this string colorRGB)
        {
            string[] cols = colorRGB.Split(',');
            return Color.FromArgb(Convert.ToInt16(cols[0]), Convert.ToInt16(cols[1]), Convert.ToInt16(cols[2]));
        }

        public static String ToRGBString(this Color color)
        {
            return color.R + "," + color.G + "," + color.B;
        }
    }
}