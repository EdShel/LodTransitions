using System.Globalization;

namespace LodTransitions.Utilities
{
    public static class NumbersExtensions
    {
        public static string ToStringWithDot(this float value)
        {
            return value.ToString("0.000000", CultureInfo.InvariantCulture);
        }
        public static string ToStringWithDot(this double value)
        {
            return value.ToString("0.000000", CultureInfo.InvariantCulture);
        }
    }
}