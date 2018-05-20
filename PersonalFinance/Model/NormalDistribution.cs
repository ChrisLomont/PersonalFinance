using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.PersonalFinance.Model
{
    [Serializable]
    public class NormalDistribution : IDistribution
    {
        public NormalDistribution(double mean = 0, double standardDeviation = 1)
        {
            Mean = mean;
            Parameter = standardDeviation;
        }

        public double Mean { get; set; } = 0;
        public double Parameter { get; set; } = 1;

        bool generate = true;
        double z1;
        public double Sample(LocalRandom random)
        {
            const double epsilon = Double.Epsilon;

            generate = !generate;

            if (!generate)
                return z1 * Parameter + Mean;

            double u1, u2;
            do
            {
                u1 = random.NextDouble();
                u2 = random.NextDouble();
            }
            while (u1 <= epsilon);

            var z0 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2*Math.PI * u2);
            z1 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2*Math.PI * u2);
            return z0 * Parameter + Mean;
        }

    }
}
