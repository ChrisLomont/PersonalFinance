using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.PersonalFinance.Model
{
    [Serializable]
    class Debt
    {
        public Debt(string name, ItemType type)
        {
            Name = name;
            Type = type;

        }
        public string Name;
        public ItemType Type;
        public double Principal;
        public double AnnualRate;
        public double Payments;
        public double MonthlyPayment;
    }
}
