using System;

namespace Lomont.PersonalFinance.Model.Distributions
{
    [Serializable]
    static class LaplaceDistribution
    {
        public static double Sample(LocalRandom random, double mean = 0, double parameter = 1)
        {
            var u = random.NextDouble()-0.5;
            return mean - parameter * Math.Sign(u) * Math.Log(1-2*Math.Abs(u));
        }
    }
}
