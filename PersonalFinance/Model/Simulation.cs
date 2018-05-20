using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lomont.PersonalFinance.Model.Distributions;
using Lomont.PersonalFinance.Model.Item;

namespace Lomont.PersonalFinance.Model
{
    public static class Simulation
    {

        public static void InitState(State state, ulong randSeed)
        {
            var random = new LocalRandom(true, randSeed);

            foreach (var rate in state.Rates)
                rate.Init(random, (int)state.StartYear, (int)state.NumYears);

        }

        /// <summary>
        /// Run the simulation on the state
        /// </summary>
        /// <param name="state"></param>
        /// <param name="randSeed"></param>
        /// <param name="log"></param>
        public static void Simulate(State state, ulong randSeed, DDataTable log)
        {
            InitState(state, randSeed);

            for (var i = 0; i < state.NumYears; ++i)
                if (!Step(state, log))
                    return;
        }

        
        public static Col AccumulatedInflationColumnName = Col.Make(ColType.Info, "Accumulated inflation");

        /// <summary>
        /// Step one year
        /// </summary>
        /// <param name="state"></param>
        /// <param name="log"></param>
        static bool Step(State state, DDataTable log)
        {
            // advance time
            state.CurrentYear++;

            // record values
            log.StartRow();
            log.AddData(state.CurrentYear, Col.Make(ColType.Info, "Year"));

            var inflation = StateUtils.GetRate(state.Rates, State.InflationName, (int)state.CurrentYear);
            log.AddData(inflation, Col.Make(ColType.Info, "InflationModel"));
            log.AddData(StateUtils.TotalRate((int)state.StartYear, (int)state.CurrentYear, StateUtils.GetRateItem(state.Rates, State.InflationName)), AccumulatedInflationColumnName);

            log.AddData(StateUtils.GetRate(state.Rates,State.StockReturnsName, (int)state.CurrentYear), Col.Make(ColType.Info, "Stock returns"));
            log.AddData(StateUtils.TotalRate((int)state.StartYear, (int)state.CurrentYear, StateUtils.GetRateItem(state.Rates, State.StockReturnsName)), Col.Make(ColType.Info, "Accum stock returns"));

            foreach (var actor in state.Actors)
            {
                actor.Age++;
                if (!actor.Step(log, inflation, state))
                    return false; // step failed - bankrupt
            }
            return true;
        }


    }
}
