using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.PersonalFinance.Model
{
    [Serializable]
    class Asset
    {
        public Asset(string name, ItemType type)
        {
            Name = name;
            Type = type;

        }
        public string Name;
        public ItemType Type;

        public double Value;
        public RateType RateType;

        // compute growth, return increase
        public double Grow(Rate rate, int year)
        {
            var rate1 = rate.GetValue(year);
            var increase = ((int) (Value * rate1 * 100))/100.0; // round to dollar
            Value += increase;
            return increase;
        }
    }
}
