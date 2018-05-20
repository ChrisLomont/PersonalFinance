using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.PersonalFinance.Model.Legal
{
    [Serializable]
    public class Taxation
    {
        public Form1040 Federal = new Form1040();
        public StateTaxes State = new StateTaxes();

        public double ComputeTax(DDataTable log, double compoundInflation)
        {
            var fed = Federal.Compute(log, compoundInflation);
            State.AdjustedGrossIncome = Federal.AdjustedGrossIncome;
            State.Exemptions = Federal.Exemptions;
            var state = State.Compute(log, compoundInflation);
            return fed + state;
        }
    }
}
