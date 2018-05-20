// #define INTERNAL_STATE
using System;
using System.Collections.Generic;
using System.Linq;
using Lomont.PersonalFinance.Model.Distributions;
using Lomont.PersonalFinance.Model.Economy;
using Lomont.PersonalFinance.Model.Item;
using Lomont.PersonalFinance.Model.Legal;


namespace Lomont.PersonalFinance.Model
{


    [Serializable]

    public class State
    {
        public static string InflationName = "Inflation rate";
        public static string StockReturnsName = "Stockmarket rate";
        public static string NoGrowthName = "No growth rate";


        public string Version = "1.0"; // file/code version

        [Variable(2017,2100,1, "Start year")]
        public double StartYear = 2017;
        public double CurrentYear = 2017;
        [Variable(1, 100, 1, "Number of years")]
        public double NumYears = 60;

        public List<Rate> Rates = new List<Rate>
        {
#if INTERNAL_STATE
            // mean CAGR of S&P 500 1928-2015
            // 19.7% std dev
            // Uses Laplace dist
            // https://sixfigureinvesting.com/2016/03/modeling-stock-market-returns-with-laplace-distribution-instead-of-normal/
            // normal https://www.ifa.com/articles/does_this_machine_simulate_market_returns/
            // new Rate(RateType.StockModel, 0.07,0.00, new StockModel()), // todo - this goes crazy 0.095, 0.197),

            new Rate(StockReturnsName,  RateDistribution.NormalDistribution, 0.095, 0.197), 
            
            // https://www.bogleheads.org/forum/viewtopic.php?t=147583
            new Rate(InflationName, RateDistribution.InflationModel, 0.03, 0.013),
            new Rate(NoGrowthName,RateDistribution.NormalDistribution, 0.0, 0.0),// no growth

            // todo - model mixed portfolios (120-age rule?)
            // https://personal.vanguard.com/us/insights/saving-investing/model-portfolio-allocations
#endif
        };

        public List<Actor> Actors = new List<Actor>
        {
        };


    }
}
