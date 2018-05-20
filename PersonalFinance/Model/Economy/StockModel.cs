using System;
using Lomont.PersonalFinance.Model.Distributions;

namespace Lomont.PersonalFinance.Model.Economy
{
    public static class StockModel
    {
        public static double Sample(LocalRandom random)
        {
            return InvestmentTables.SampleSeries(1928, 2016, random, InvestmentTables.sp500Returns1928To2016);
        }
    }
}
