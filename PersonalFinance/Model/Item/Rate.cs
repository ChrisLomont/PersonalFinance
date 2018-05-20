using System;
using Lomont.PersonalFinance.Model.Distributions;
using Lomont.PersonalFinance.Model.Economy;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lomont.PersonalFinance.Model.Item
{
    public enum RateDistribution
    {
        NormalDistribution,
        LaplaceDistribution,
        InflationModel,
        StockModel,
        CompoundInterest,
    }


    [Serializable]
    // store a rate for each of many years
    public class Rate
    {
        public Rate(string name, RateDistribution rateDistribution, params double [] parameters)
        {
            RateDistribution = rateDistribution;
            Name = name;
            Parameters = parameters;
        }

        public double[] Parameters;

        public string Name;

        [JsonConverter(typeof(StringEnumConverter))]
        public RateDistribution RateDistribution;

        int startYear;
        double[] values;

        public double GetValue(int year)
        {
            if (values != null)
                return values[year - startYear];
            return 0.0;
        }

        public void Init(LocalRandom random, int startYear, int numYears)
        {
            this.startYear = startYear;
            values = new double[numYears+1];
            for (var i = 0; i < values.Length; ++i)
                values[i] = Sample(random);
        }

        double Sample(LocalRandom random)
        {
            // NOTE: don't use an interface and OO to select, makes JSON serialization a big mess
            switch (RateDistribution)
            {
                case RateDistribution.NormalDistribution:
                    return NormalDistribution.Sample(random, Parameters[0], Parameters[1]);
                case RateDistribution.LaplaceDistribution:
                    return LaplaceDistribution.Sample(random, Parameters[0], Parameters[1]);
                case RateDistribution.InflationModel:
                    return InflationModel.Sample(random);
                case RateDistribution.StockModel:
                    return StockModel.Sample(random);
                default:
                    throw new NotImplementedException("Unknown RateType in Rate.Sample");
            }
        }
    }
}
