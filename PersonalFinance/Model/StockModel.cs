using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.PersonalFinance.Model
{
    [Serializable]
    class StockModel : IDistribution
    {
        public double Mean { get; set; }
        public double Parameter { get; set; }
        public double Sample(LocalRandom random)
        {
            return InvestmentTables.SampleSeries(1928, 2016, random, InvestmentTables.sp500Returns1928To2016);
        }
    }
}
