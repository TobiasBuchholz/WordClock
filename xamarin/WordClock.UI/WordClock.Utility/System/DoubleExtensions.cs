namespace System
{
    public static class DoubleExtensions
    {
        public static bool NearlyEquals(this double first, double second)
        {
            return Math.Abs(first - second) < 1E-15;
        }
    }
}