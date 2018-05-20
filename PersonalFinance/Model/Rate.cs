using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.PersonalFinance.Model
{
    [Serializable]
    // store a rate for each of many  years
    class Rate
    {
        public Rate(RateType rateType, double mean = 0, double standardDeviation = 0, IDistribution distribution = null)
        {
            Type = rateType;
            if (distribution == null)
                pdf = new NormalDistribution(mean, standardDeviation);
            else
                pdf = distribution;
        }

        public IDistribution pdf;

        public RateType Type;

        int startYear;
        double[] values;

        public double GetValue(int year)
        {
            return values[year - startYear];
        }

        public void Init(LocalRandom random, int startYear, int numYears)
        {
            this.startYear = startYear;
            values = new double[numYears];
            for (var i = 0; i < numYears; ++i)
                values[i] = pdf.Sample(random);
        }
    }
}
