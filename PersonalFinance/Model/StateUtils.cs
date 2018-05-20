using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lomont.PersonalFinance.Model.Item;
using Lomont.PersonalFinance.Model.Legal;

namespace Lomont.PersonalFinance.Model
{
    public static class StateUtils
    {
        /// <summary>
        /// Compute mortgages for each actor
        /// </summary>
        internal static void ComputeMortgage(State state, ulong randSeed)
        {
            Simulation.InitState(state, randSeed);

            foreach (var actor in state.Actors)
            {
                var mortgate = actor.GetDebt(AssetType.Mortgage);
                if (mortgate != null)
                {
                    var principal = mortgate.Principal;
                    var monthlyRate = mortgate.AnnualRate / 12;
                    var numPayments = mortgate.Payments;
                    var pow = Math.Pow(1 + monthlyRate, numPayments);
                    var payment = principal * monthlyRate * pow / (pow - 1);
                    mortgate.MonthlyPayment = payment;
                }
            }
        }

        /// <summary>
        /// Compute social security payments for all actors
        /// </summary>
        internal static void ComputeSocialSecurity(State state, ulong randSeed)
        {
            Simulation.InitState(state, randSeed);

            foreach (var actor in state.Actors)
            {
                var monthlySs = SocialSecurity.ComputeSocialSecurity(
                    actor.YearlyWages,
                    actor.WageStartYear,
                    actor.AgeAtStartYear,
                    (int)actor.RetirementAge,
                    year => GetRate(state.Rates, State.InflationName, year)
                    );
                actor.MonthlySocialSecurityIncome = monthlySs;
            }
        }

        public static Rate GetRateItem(IEnumerable<Rate> rates, string rateName)
        {
            if (rates.Any())
                return rates.First(r => r.Name == rateName);
            return new Rate(State.InflationName, RateDistribution.InflationModel, 0.03, 0.013);
        }

        public static double GetRate(IEnumerable<Rate> rates, string rateName, int year)
        {
            return GetRateItem(rates, rateName).GetValue(year);
        }

        public static double TotalRate(int startYear, int currentYear, Rate rate)
        {
            var r = 1.0;
            for (var y = startYear; y <= currentYear; y++)
                r *= rate.GetValue(y) + 1;
            return r;
        }


    }
}
