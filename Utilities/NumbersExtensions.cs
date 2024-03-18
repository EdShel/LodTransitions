using System.Globalization;

namespace LodTransitions.Utilities
{
    public static class NumbersExtensions
    {
        public static string ToStringWithDot(this float value)
        {
            return value.ToString("0.0", CultureInfo.InvariantCulture);
        }
    }
}