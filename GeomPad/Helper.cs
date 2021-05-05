using System.Globalization;


namespace GeomPad
{
    public static class Helper
    {
        public static float ParseFloat(this string f)
        {
            return float.Parse(f.Replace(",", "."), CultureInfo.InvariantCulture);
        }
    }

    
}
