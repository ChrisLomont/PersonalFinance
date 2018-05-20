using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Lomont.PersonalFinance.Model.Distributions;

namespace Lomont.PersonalFinance.Model
{
    class MonteCarloSim
    {
        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }


        /// <summary>
        /// return logger that has data for monte carlo sim
        /// todo - count, parallel
        /// </summary>
        /// <param name="state"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static async Task<Tuple<GDataTable,double[]>> RunSimulations(State state, int count, ulong randomSeed, double [] percentiles)
        {
            var logs = new ConcurrentQueue<DDataTable>();
            await Task.Run(() =>
            {
                var rand1 = new LocalRandom(true, randomSeed);
                var seeds = new ulong[count];
                for (var i = 0; i < count; ++i)
                    seeds[i] = rand1.Next64();
                Parallel.For(0, count, n =>
                {
                    var log = new DDataTable();
                    var tempState = DeepClone(state);

                    Simulation.Simulate(tempState, seeds[n], log);

                    logs.Enqueue(log);
                });
            });

            var finalLog = new GDataTable();
            if (count == 1)
            { // create log of proper type
                var log = logs.First();
                foreach (var row in Enumerable.Range(0,log.RowCount))
                {
                    finalLog.StartRow();
                    foreach (var col in Enumerable.Range(0,log.ColCount))
                        finalLog.AddData(new double[] { log[row,col] }, log.ColumnNames[col]);
                }
                return new Tuple<GDataTable, double[]>(finalLog, new double[] {0.50});
            }

            // sort by sum of total worths in item
            var sortedLogs = new List<Tuple<DDataTable,double>>();
            foreach (var log in logs)
            {
                var maxCol = log.ColumnNames.Count;
                var lastRow = log.RowCount - 1;
                var value = 0.0;
                for (var i = 0; i < maxCol; ++i)
                {
                    if (log.ColumnNames[i].ToString().Contains(Actor.FinalValueText))
                        value += log[lastRow,i];
                }
                sortedLogs.Add(new Tuple<DDataTable, double>(log,value));
            }
            sortedLogs.Sort((a, b) => a.Item2.CompareTo(b.Item2));


            var rowMax = sortedLogs.Max(r => r.Item1.RowCount);
            var columnNames = sortedLogs[0].Item1.ColumnNames;
            foreach (var row in Enumerable.Range(0, rowMax+1))
            {
                finalLog.StartRow();
                foreach (var name in columnNames)
                {
                    var value = new List<double>();
                    foreach (var entry in percentiles)
                    {
                        var log = sortedLogs[(int)(sortedLogs.Count * entry)].Item1;
                        var column = log.GetColumn(name);
                        if (column.Count > row)
                            value.Add(column[row]);
                    }
                    finalLog.AddData(value.ToArray(), name);
                }
            }

            return new Tuple<GDataTable, double[]>(finalLog, percentiles);
        }

    }
}
