using System;
using Lomont.PersonalFinance.Model.Distributions;

namespace Lomont.PersonalFinance.Model.Economy
{
    public static class InflationModel
    {

        // http://www.thinkadvisor.com/2014/10/20/using-monte-carlo-for-inflation-assumptions-in-fin
        // uses lognormal fit to this annual inflation from 1947 through 2013
        // range: <0  0-1  1-2  2-3  3-4  4-5  5-6  6-7  7-8  8+
        // count:  3   3    13   13   13   6    4    3    3   5
        // location -2.88, mean 3.66, std dev = 2.81

        // historical tables 
        // http://www.usinflationcalculator.com/inflation/historical-inflation-rates/
        // 1957-2016 annual inflation?!
        static double[] annualInflation1957To2016 =
        {
            3.3,
            2.8,
            0.7,
            1.7,
            1,
            1,
            1.3,
            1.3,
            1.6,
            2.9,
            3.1,
            4.2,
            5.5,
            5.7,
            4.4,
            3.2,
            6.2,
            11,
            9.1,
            5.8,
            6.5,
            7.6,
            11.3,
            13.5,
            10.3,
            6.2,
            3.2,
            4.3,
            3.6,
            1.9,
            3.6,
            4.1,
            4.8,
            5.4,
            4.2,
            3.0,
            3.0,
            2.6,
            2.8,
            3,
            2.3,
            1.6,
            2.2,
            3.4,
            2.8,
            1.6,
            2.3,
            2.7,
            3.4,
            3.2,
            2.8,
            3.8,
            -0.4,
            1.6,
            3.2,
            2.1,
            1.5,
            1.6,
            0.1,
            1.3
        };


        public static double Sample(LocalRandom random)
        {
            var len = annualInflation1957To2016.Length;
            return annualInflation1957To2016[random.Next(len)]/100.0;
        }
    }
}
