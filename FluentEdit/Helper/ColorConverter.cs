using System;
using Windows.UI;

namespace FluentEdit.Helper
{
    public class ColorConverter
    {
        public static Color ToColorWithAlpha(string clr, Color defaultClr)
        {
            if (clr.Length != 8)
                return defaultClr;

            byte a = Convert.ToByte(clr.Substring(0, 2), 16);
            byte r = Convert.ToByte(clr.Substring(2, 2), 16);
            byte g = Convert.ToByte(clr.Substring(4, 2), 16);
            byte b = Convert.ToByte(clr.Substring(6, 2), 16);

            return Color.FromArgb(a, r, g, b);
        }
        public static Color ToColor(string clr, Color defaultClr)
        {
            if (clr.Length != 6)
                return defaultClr;

            byte r = Convert.ToByte(clr.Substring(0, 2), 16);
            byte g = Convert.ToByte(clr.Substring(2, 2), 16);
            byte b = Convert.ToByte(clr.Substring(4, 2), 16);

            return Color.FromArgb(255, r, g, b);
        }

        public static string ToStringWithAlpha(Color clr)
        {
            return $"{clr.A:X2}{clr.R:X2}{clr.G:X2}{clr.B:X2}";
        }

        public static string ToString(Color clr)
        {
            return $"{clr.R:X2}{clr.G:X2}{clr.B:X2}";
        }

    }
}
