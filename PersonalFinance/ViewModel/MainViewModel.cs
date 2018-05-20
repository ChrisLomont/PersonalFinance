using GalaSoft.MvvmLight;
using Lomont.PersonalFinance.Model;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using System.Windows;
using System.Windows.Threading;
using Newtonsoft.Json;
using System.IO;
using Lomont.PersonalFinance.Model.Legal;
using Lomont.PersonalFinance.Model.Item;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Lomont.PersonalFinance.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
            OpenModelCommand = new RelayCommand(OpenModel);
            ComputeMortgateCommand = new RelayCommand(ComputeMortgage);
            ComputeSocialSecurityCommand = new RelayCommand(ComputeSocialSecurity);
            ExportCsvCommand = new RelayCommand(ExportCsv);
            AddVariableCommand = new RelayCommand(AddVariable);
            RemoveVariableCommand = new RelayCommand(RemoveVariable);
            RunMonteCarloCommand = new RelayCommand(MonteCarlo);


            updateTimer.Tick += (o, e) => CheckUpdates();
            updateTimer.Interval = TimeSpan.FromMilliseconds(50);
            updateTimer.Start();


        }
        public ICommand ComputeSocialSecurityCommand { get; private set;  }
        public ICommand OpenModelCommand { get; private set; }
        public ICommand ComputeMortgateCommand { get; private set; }
        public ICommand ExportCsvCommand { get; private set; }
        public ICommand RunMonteCarloCommand { get; private set; }
        public ICommand AddVariableCommand { get; private set; }
        public ICommand RemoveVariableCommand { get; private set; }
        public ObservableCollection<string> GraphVariables { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> BoundGraphVariables { get; } = new ObservableCollection<string>();


        #region SelectedGraphVariable Property
        string selectedGraphVariable = null;
        public string SelectedGraphVariable
        {
            get => selectedGraphVariable;
            set => Set(() => SelectedGraphVariable, ref selectedGraphVariable, value);
        }
        #endregion

        #region SelectedBoundGraphVariable Property
        string selectedBoundGraphVariable = null;
        public string SelectedBoundGraphVariable
        {
            get => selectedBoundGraphVariable;
            set => Set(() => SelectedBoundGraphVariable, ref selectedBoundGraphVariable, value);
        }
        #endregion

        #region PercentilesText Property
        // what ranges do we output?
        string percentilesText = "5 10 25 50 75 90 95";
        public string PercentilesText
        {
            get => percentilesText;
            set
            {
                if (Set(() => PercentilesText, ref percentilesText, value))
                    UpdatePercentiles();
            }
        }
        #endregion

        #region MonteCarloCount Property
        int monteCarloCount = 1000;
        public int MonteCarloCount
        {
            get => monteCarloCount;
            set => Set(() => MonteCarloCount, ref monteCarloCount, value);
        }
        #endregion
        #region RandomSeed Property
        ulong randomSeed = 1234567;
        public ulong RandomSeed
        {
            get => randomSeed;
            set => Set(() => RandomSeed, ref randomSeed, value);
        }
        #endregion

        #region InflationAdjusted Property
        bool inflationAdjusted = true;
        public bool InflationAdjusted
        {
            get => inflationAdjusted;
            set
            {
                if (Set(() => InflationAdjusted, ref inflationAdjusted, value))
                    DrawData();
            }

        }
        #endregion

        /// <summary>
        /// Add selected variable to graph
        /// </summary>
        public void AddVariable()
        {
            var s = SelectedGraphVariable;
            if (!String.IsNullOrEmpty(s) && !BoundGraphVariables.Contains(s))
            {
                BoundGraphVariables.Add(s);
                DrawData();
            }
        }

        /// <summary>
        /// Remove selected variable from graph
        /// </summary>
        public void RemoveVariable()
        {
            var s = SelectedBoundGraphVariable;
            if (!String.IsNullOrEmpty(s) && BoundGraphVariables.Contains(s))
            {
                BoundGraphVariables.Remove(s);
                DrawData();
            }
        }

        void ExportCsv()
        {
            var sb = new StringBuilder();
            foreach (var c in FinanceTable.Columns)
                sb.Append(c.ToString() + ",");
            sb.AppendLine();
            foreach (DataRow r in FinanceTable.Rows)
            {
                foreach (DataColumn c in FinanceTable.Columns)
                    sb.Append(r[c] + ",");
                sb.AppendLine();
            }

            Clipboard.SetText(sb.ToString());
        }

        void OpenModel()
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                try
                {                 // todo - Json testing
                    var settings = new JsonSerializerSettings
                    {
                        // allows saving interfaces, but breaks long term file portability.
                        //TypeNameHandling = TypeNameHandling.All
                    };
                    //var jsonText = JsonConvert.SerializeObject(state, Formatting.Indented, settings);
                    //File.WriteAllText("Model.txt", jsonText);

                    // cannot instantiate due to interfaces being used - need concrete objects or types or enums
                    var jsonText = File.ReadAllText(ofd.FileName);
                    var state2 = JsonConvert.DeserializeObject<State>(jsonText, settings);
                    state = state2;

                    ScheduleUpdate();
                    AddVariables();

                    SocialSecurity.Test();

                    UpdatePercentiles();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception: " + ex);
                }
            }

        }



        void ComputeSocialSecurity()
        {
            StateUtils.ComputeSocialSecurity(state, RandomSeed);
            Update(1);
            Variables.Clear();
            AddVariables();
        }

        void ComputeMortgage()
        {
            StateUtils.ComputeMortgage(state, RandomSeed);

            Update();
            Variables.Clear();
            AddVariables();
        }

        void MonteCarlo()
        {
            Update(MonteCarloCount);
        }

        void NotImplemented()
        {
            MessageBox.Show("Not Implemented");
        }

        void AddVariables()
        { // todo - reflect on attributes
            

            // variables to scope
            Variables.Add(new BoundValue(
                "Start year",
                2017, 2100, 1,
                () => state.StartYear,
                v => state.StartYear = v,
                ScheduleUpdate
                ));
            Variables.Add(new BoundValue(
                "Num years",
                1, 100, 1,
                () => state.NumYears,
                v => state.NumYears = v,
                ScheduleUpdate
                ));

            var inflation = StateUtils.GetRateItem(state.Rates,State.InflationName);
            Variables.Add(new BoundValue(
                "InflationModel mean",
                -0.15, 0.15, 0.005,
                () => inflation.Parameters[0],
                v => inflation.Parameters[0] = v,
                ScheduleUpdate
                ));
            Variables.Add(new BoundValue(
                "InflationModel std dev",
                -0.15, 0.15, 0.005,
                () => inflation.Parameters[1],
                v => inflation.Parameters[1] = v,
                ScheduleUpdate
                ));


            foreach (var actor in state.Actors)
                AddActorVars(actor);
        }

        void AddActorVars(Actor actor)
        { // todo - reflect
            var name = actor.Name;
            Variables.Add(new BoundValue(
                $"{name} Retirement Age",
                20, 100, 1,
                () => actor.RetirementAge,
                v => actor.RetirementAge = v,
                ScheduleUpdate
                ));
            Variables.Add(new BoundValue(
                $"{name} Social Security Age",
                20, 100, 1,
                () => actor.SocialSecurityAge,
                v => actor.SocialSecurityAge = v,
                ScheduleUpdate
                ));
            Variables.Add(new BoundValue(
                $"{name} Annual Wages",
                0, 250000, 1000,
                () => actor.AnnualWageIncome,
                v => actor.AnnualWageIncome = v,
                ScheduleUpdate
                ));
            Variables.Add(new BoundValue(
                $"{name} Monthly Soc Sec",
                0, 20000, 100,
                () => actor.MonthlySocialSecurityIncome,
                v => actor.MonthlySocialSecurityIncome = v,
                ScheduleUpdate
                ));
            Variables.Add(new BoundValue(
                $"{name} Annual other taxable income",
                0, 20000, 100,
                () => actor.AnnualOtherTaxableIncome,
                v => actor.AnnualOtherTaxableIncome = v,
                ScheduleUpdate
                ));
            Variables.Add(new BoundValue(
                $"{name} Annual other untaxable income",
                0, 20000, 100,
                () => actor.AnnualOtherUntaxableIncome,
                v => actor.AnnualOtherUntaxableIncome = v,
                ScheduleUpdate
                ));
            Variables.Add(new BoundValue(
                $"{name} Annual 401k contribution",
                0, 50000, 1000,
                () => actor.Annual401KContribution,
                v => actor.Annual401KContribution = v,
                ScheduleUpdate
                ));
            Variables.Add(new BoundValue(
                $"{name} Monthly expenses",
                0, 10000, 100,
                () => actor.MonthlyExpenses,
                v => actor.MonthlyExpenses = v,
                ScheduleUpdate
                ));
            Variables.Add(new BoundValue(
                $"{name} Annual other expenses",
                0, 40000, 1000,
                () => actor.AnnualOtherExpenses,
                v => actor.AnnualOtherExpenses = v,
                ScheduleUpdate
                ));

            Variables.Add(new BoundValue(
                $"{name} Cash on hand",
                0, 10000, 500,
                () => actor.GetAsset(AssetType.Cash).Value,
                v => actor.GetAsset(AssetType.Cash).Value = v,
                ScheduleUpdate
                ));
            Variables.Add(new BoundValue(
                $"{name} Desired cash on hand",
                0, 10000, 500,
                () => actor.DesiredCashOnHand,
                v => actor.DesiredCashOnHand = v,
                ScheduleUpdate
                ));
            // todo - more items: 401k, retirement, etc
            if (actor.GetDebt(AssetType.Mortgage) != null)
            {
                Variables.Add(new BoundValue(
                    $"{name} Mortgage",
                    0, 1000000, 10000,
                    () => actor.GetDebt(AssetType.Mortgage).Principal,
                    v =>
                    {
                        actor.GetDebt(AssetType.Mortgage).Principal = v;
                        actor.GetAsset(AssetType.House).Value = v;
                    },
                    ScheduleUpdate
                ));
                Variables.Add(new BoundValue(
                    $"{name} Mortgage rate %",
                    0.01, 0.10, 0.005,
                    () => actor.GetDebt(AssetType.Mortgage).AnnualRate,
                    v => actor.GetDebt(AssetType.Mortgage).AnnualRate = v,
                    ScheduleUpdate
                ));
                Variables.Add(new BoundValue(
                    $"{name} Mortgage # payments",
                    1, 500, 1,
                    () => actor.GetDebt(AssetType.Mortgage).Payments,
                    v => actor.GetDebt(AssetType.Mortgage).Payments = v,
                    ScheduleUpdate
                ));
                Variables.Add(new BoundValue(
                    $"{name} Mortgage monthly",
                    0, 10000, 100,
                    () => actor.GetDebt(AssetType.Mortgage).MonthlyPayment,
                    v => actor.GetDebt(AssetType.Mortgage).MonthlyPayment = v,
                    ScheduleUpdate
                ));
            }
        }

        public SeriesCollection SeriesCollection { get;  } = new SeriesCollection();
        #region Labels Property 
        public string[] labels = null;

        public string[] Labels
        {
            get => labels;
            set => Set(()=>Labels,ref labels, value);
        }
        #endregion
        public Func<double, string> YFormatter { get; set; }

        public ObservableCollection<BoundValue> Variables { get;  } = new ObservableCollection<BoundValue>(); 

        State state = new State();

        #region FinanceTable Property
        /// <summary>
        /// Bindable table for the datagrid
        /// </summary>
        DataTable financeTable = new DataTable("Finances");
        public DataTable FinanceTable
        {
            get => financeTable;
            set => Set(() => FinanceTable, ref financeTable, value);
        }
        #endregion 

        /// <summary>
        /// the last data table returned from the simulation
        /// Is original data used to make the FinanceTable
        /// </summary>
        GDataTable dataTable; 

        double Format(double value, string text)
        {
            return Math.Round(value * 100) / 100.0;
        }

        DispatcherTimer updateTimer = new DispatcherTimer();

        int ticksTillUpdate = 0;

        void CheckUpdates()
        {
            if (ticksTillUpdate > 0)
            {
                --ticksTillUpdate;
                if (ticksTillUpdate == 0)
                    Update();
            }
        }

        void ScheduleUpdate()
        {
            ticksTillUpdate = 5;
        }

        double[] percentiles;
        void UpdatePercentiles()
        {
            try
            {
                var words = PercentilesText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var vals = words.Select(w => Double.Parse(w)/100.0).ToArray();
                percentiles = vals;
            }
            catch 
            {
                // todo - message?
            }
        }

        async void Update(int count = 1)
        {
            (dataTable,_) = await MonteCarloSim.RunSimulations(state, count, RandomSeed, percentiles);

            // if variables changed, then update UI
            if (GraphVariables.Count != dataTable.ColumnNames.Count)
            {
                GraphVariables.Clear();
                BoundGraphVariables.Clear();
                foreach (var name in dataTable.ColumnNames)
                    GraphVariables.Add(name.ToString());
            }


            FinanceTable = CreateTable(dataTable);
            DrawData();
        }

        /// <summary>
        /// Given the data log from the simulation, create a matching data table for binding
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        static DataTable CreateTable(GDataTable log)
        {
            var dt = new DataTable("Finances");
            dt.Columns.Clear();
            dt.Clear();

            foreach (var name in log.ColumnNames)
                dt.Columns.Add(name.ToString());
            foreach (var row in Enumerable.Range(0, log.RowCount))
            {
                var r = dt.NewRow();
                for (var col = 0; col < log.ColumnNames.Count; ++col)
                {
                    var tup = log[row,col];
                    if (tup.Length == 1)
                        r[log.ColumnNames[col].ToString()] = $"{tup[0]:F2}";
                    else if (tup.Length > 1)
                    {
                        var firstVal = tup[0];
                        //if (tup.All(v => v == firstVal))
                        //    r[log.ColumnNames[j].ToString()] = $"{tup[0]:F2}";
                        //else
                        {
                            var sb = new StringBuilder();
                            foreach (var v in tup)
                                sb.Append($", {v:F2}");
                            r[log.ColumnNames[col].ToString()] = sb.ToString().Substring(2);
                        }
                    }
                }
                dt.Rows.Add(r);
            }
            return dt;
        }


        /// <summary>
        /// Parse comma delimited string of doubles into array
        /// </summary>
        /// <param name="tuple"></param>
        /// <returns></returns>
        void GetTuple(double [] vals, int num, string text)
        {
            var words = text.Split(',').Select(s => s.Trim()).ToArray();
            if (vals.Length != words.Length || vals.Length != num)
                throw new Exception("Mismatched array size in get tuple");
            for (var i = 0; i < num; ++i)
                vals[i] = Double.Parse(words[i]);
        }

        uint ShowPercentile = 0xFFFFFFFF; // kth bit set means show kth series - todo - implement

        /// <summary>
        /// Draw data to graph
        /// </summary>
        void DrawData()
        {
            // how many percentiles are computed by the Monte Carlo
            var tupleSize = dataTable[0, 0].Length;
            var rowCount = dataTable.RowCount;

            // year labels
            if (Labels == null || Labels.Length != rowCount)
                Labels = new string[rowCount];
            for (var i = 0; i < rowCount; ++i)
            {
                var label = $"{state.StartYear + i}";
                foreach (var actor in state.Actors)
                    label += $"\n{actor.Age + i}";
                Labels[i] = label;
            }
            
            // trigger update by setting null and back
            var tempLabels = Labels;
            Labels = null;
            Labels = tempLabels; // trigger update

            // get inflation
            List<double[]> accumulatedInflation = null;
            if (InflationAdjusted)
            {
                var cols = dataTable.GetColumn(Simulation.AccumulatedInflationColumnName);
                accumulatedInflation = SplitColumns(cols);
            }

            var makeNew = false;
            if (SeriesCollection == null || SeriesCollection.Count != BoundGraphVariables.Count*tupleSize || !LabelsMatch())
                makeNew = true;

            // data
            if (makeNew)
                SeriesCollection.Clear();

            var colNames = dataTable.ColumnNames;

            var seriesIndex = 0;
            // iterator over things requested to draw
            foreach (var colName in BoundGraphVariables)
            {
                // we draw (potentially) one chart per item in tuple
                var vals = new ChartValues<double>[tupleSize];
                for (var pct = 0; pct < tupleSize; ++pct)
                    vals[pct] = new ChartValues<double>();


                // create list of list of doubles, each a column of data
                var colItem = dataTable.ColumnNames.Where(n => n.ToString() == colName).First();
                var colData = SplitColumns(dataTable.GetColumn(colItem));

                // add these to the charts
                for (var tt = 0; tt < tupleSize; ++tt)
                {
                    vals[tt].AddRange(colData[tt]);
                    if (InflationAdjusted)
                    {
                        for (var row = 0; row < vals[tt].Count; ++row)
                            vals[tt][row] /= accumulatedInflation[tt][row];
                    }
                }

                if (makeNew)
                {
                    var i = 0;
                    foreach (var c in vals)
                    {
                        SeriesCollection.Add(
                            new LineSeries
                            {
                                Title = colName + $" ({(int)(percentiles[i]*100)}%)",  
                                Values = c
                                //PointGeometry = DefaultGeometries.Square,
                                // PointGeometrySize = 15
                            });
                        ++i;
                        ++seriesIndex;
                    }
                }
                else
                {
                    foreach (var c in vals)
                    {
                        for (var j = 0; j < c.Count; ++j)
                            SeriesCollection[seriesIndex].Values[j] = vals[j];
                        ++seriesIndex;
                    }
                }
            }

            YFormatter = value => value.ToString("C");

            // see if series data labels match BoundGraphVariables
            bool LabelsMatch()
            {
                for (var i = 0; i < SeriesCollection.Count; ++i)
                    if (((LineSeries)SeriesCollection[i]).Title != BoundGraphVariables[i])
                        return false;
                return true;
            }
            // given a list of double[], compute the transpose as a List of double[]
            List<double[]> SplitColumns(List<double[]> matrix)
            {
                var ans = new List<double[]>();
                var r = matrix.Count;
                var c = matrix[0].Length;
                for (var j = 0; j < c; ++j)
                    ans.Add(new double[r]);

                for (var j = 0; j < c; ++j)
                {
                    for (var i = 0; i < r; ++i)
                    {
                        if (matrix[i].Length > j)
                            ans[j][i] = matrix[i][j];
                    }
                }
                return ans;
            }


        }


        bool Contains(string text, string startText, string containsText)
        {
            return text.StartsWith(startText) && text.Contains(containsText);
        }

    }
}