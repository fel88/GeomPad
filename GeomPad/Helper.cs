using System.Globalization;

namespace GeomPad
{
    public static class Helper
    {
        public static float ParseFloat(this string f)
        {
            return float.Parse(f.Replace(",", "."), CultureInfo.InvariantCulture);
        }

        public static double ParseDouble(this string f)
        {
            return double.Parse(f.Replace(",", "."), CultureInfo.InvariantCulture);
        }
    }    
}
