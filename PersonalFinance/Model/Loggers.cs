using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.PersonalFinance.Model
{
    // nice aliases
    public class DDataTable : DataTable<double, Col>
    {
    }
    public class GDataTable : DataTable<double[], Col>
    {
    }
}
