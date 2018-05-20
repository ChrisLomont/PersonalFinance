using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.PersonalFinance.Model
{
    interface IDistribution
    {
        double Mean { get; set; }
        double Parameter { get; set; }
        double Sample(LocalRandom random);

    }
}
