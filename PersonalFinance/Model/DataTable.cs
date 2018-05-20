using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.PersonalFinance.Model
{
    /// <summary>
    /// Store a data table with named columns and multiple rows of things
    /// </summary>
    public class DataTable<T,TC> where TC: IEquatable<TC>
    {
        /// <summary>
        /// List of column names
        /// </summary>
        public List<TC> ColumnNames { get; } = new List<TC>();

        public int RowCount => values.Count;
        public int ColCount => ColumnNames.Count;

        /// <summary>
        /// Start a new row of values
        /// </summary>
        public void StartRow()
        {
            values.Add(new List<T>());
        }

        /// <summary>
        /// Index by row and column
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public T this[int row, int column]
        {
            get => values[row][column];
            set => values[row][column] = value;
        }

        /// <summary>
        /// Log the value in the next column on this row
        /// If name not present, add it to the column names
        /// </summary>
        /// <param name="value"></param>
        /// <param name="colName"></param>
        public void AddData(T value, TC colName)
        {
            var last = values.Last();
            last.Add(value);
            //if (ColumnNames.Count <= last.Count) // todo - faster?
            if (!ColumnNames.Contains(colName)) // faster
                ColumnNames.Add(colName);
        }

        internal void AddColumn(TC columnName, List<T> columnValues)
        {
            // ensure enough rows
            while (this.values.Count < columnValues.Count)
                this.values.Add(new List<T>());

            var col = ColumnNames.IndexOf(columnName);
            if (col < 0)
            {
                ColumnNames.Add(columnName);
                col = ColumnNames.Count - 1;
                // ensure enough cols
                while (values[0].Count <= col)
                {
                    foreach (var t in values)
                        t.Add(default(T));
                }
            }
            for (var row = 0; row < columnValues.Count; ++row)
                this.values[row][col] = columnValues[row];
        }

        readonly List<List<T>> values = new List<List<T>>();

        /// <summary>
        /// Get the column of data for the given name
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        internal List<T> GetColumn(TC columnName)
        { // todo - store in columns? would be faster for this lookup
            var list = new List<T>();
            var i = ColumnNames.IndexOf(columnName);
            if (i >= 0)
                list.AddRange(values.Select(row => row[i]));
            return list;
        }

        public override string ToString()
        {
            return $"({RowCount},{ColCount}) {typeof(T)}";
        }
    }
}
