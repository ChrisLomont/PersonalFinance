using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace Lomont.PersonalFinance.ViewModel
{
    public class BoundValue :ViewModelBase
    {
        public BoundValue(
            string label, 
            double min,
            double max,
            double step,
            Func<double> getVal, 
            Action<double> setVal, 
            Action update
            )
        {
            this.Label = label;
            this.Min = min;
            this.Max = max;
            Step = step;
            this.getVal = getVal;
            this.setVal = setVal;
            this.update = update;
        }

            Func<double> getVal; 
            Action<double> setVal;
        Action update;


        #region Label Property
        string label;
        public string Label
        {
            get => label;
            set => Set(() => Label, ref label, value);
        }
        #endregion


        #region Min Property
        double min;
        public double Min
        {
            get => min;
            set => Set(() => Min, ref min, value);
        }
        #endregion

        #region Max Property
        double max;
        public double Max
        {
            get => max;
            set => Set(() => Max, ref max, value);
        }
        #endregion

        #region Step Property
        double step;
        public double Step
        {
            get => step;
            set => Set(() => Step, ref step, value);
        }
        #endregion



        #region Value Property
        public double Value
        {
            get => getVal();
            set
            {
                var temp = Value;
                if (Set(() => Value, ref temp, value))
                {
                    setVal(value);
                    update();
                }
            }
        }
        #endregion



    }
}
