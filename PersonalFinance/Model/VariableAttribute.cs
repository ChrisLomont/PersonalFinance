using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.PersonalFinance.Model
{
    public class VariableAttribute :Attribute
    {
        public VariableAttribute(double minValue, double maxValue, double stepSize, string description)
        {
        }
    }
}
