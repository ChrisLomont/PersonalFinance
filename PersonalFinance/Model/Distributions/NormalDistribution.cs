using System;

namespace Lomont.PersonalFinance.Model.Distributions
{
    [Serializable]
    public static class NormalDistribution
    {
        public static double Sample(LocalRandom random, double mean, double standardDeviation)
        {
            const double epsilon = Double.Epsilon;

            // note: could get two for the price of one if not static
            //generate = !generate;
            //if (!generate)
            //    return z1 * Parameter + Mean;

            double u1, u2;
            do
            {
                u1 = random.NextDouble();
                u2 = random.NextDouble();
            }
            while (u1 <= epsilon);

            var z0 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2*Math.PI * u2);
            // var z1 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2*Math.PI * u2);
            return z0 * standardDeviation + mean;
        }

    }
}
