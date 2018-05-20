using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.PersonalFinance.Model
{
    [Serializable]
    class LaplaceDistribution :IDistribution
    {
        public LaplaceDistribution(double mean = 0, double parameter = 1)
        {
            Mean = mean;
            Parameter = parameter;
        }

        public double Mean { get; set; } = 0;
        public double Parameter { get; set; } = 1;
        public double Sample(LocalRandom random)
        {
            var u = random.NextDouble()-0.5;
            return Mean - Parameter * Math.Sign(u) * Math.Log(1-2*Math.Abs(u));
        }
    }
}
