namespace MlpProject
{
    /// <summary>
    /// Small numeric helpers shared by the evaluation classes.
    /// </summary>
    internal static class MetricMath
    {
        /// <summary>
        /// Divides two numbers, returning a fallback value instead of NaN or Infinity
        /// when the denominator is zero.
        /// </summary>
        /// <param name="numerator">The dividend.</param>
        /// <param name="denominator">The divisor.</param>
        /// <param name="valueWhenUndefined">Result returned when the denominator is zero (default 0.0).</param>
        /// <returns>The quotient, or <paramref name="valueWhenUndefined"/> if the division is undefined.</returns>
        public static double SafeDivide(double numerator, double denominator, double valueWhenUndefined = 0.0)
        {
            return denominator == 0.0 ? valueWhenUndefined : numerator / denominator;
        }
    }
}