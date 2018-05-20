using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.PersonalFinance.Model.Legal
{
    [Serializable]
    public class StateTaxes
    {
        /// <summary>
        /// From form 1040
        /// </summary>
        public  double AdjustedGrossIncome { get; set;  }

        /// <summary>
        /// From ???
        /// </summary>
        public double HomeownerPropertyTax { get; set; }

        public double TaxableSocialSecurityBenefits { get; set; }

        /// <summary>
        /// From federal form
        /// </summary>
        public int Exemptions { get; set;  }

        /// <summary>
        /// Number of people over 65 exemption: 1 for you, 1 for spouse if joint
        /// </summary>
        public int PeopleOver65 { get; set; }


        public double Compute(DDataTable log, double compoundInflation)
        {

            var totalDeductions = HomeownerPropertyTax + TaxableSocialSecurityBenefits;

            var totalExemptions = compoundInflation * Exemptions * 1000;

            totalExemptions += compoundInflation * PeopleOver65 * 1000;

            if (AdjustedGrossIncome < 40000)
                totalExemptions += compoundInflation * PeopleOver65 * 500;

            var indianaGrossIncome = AdjustedGrossIncome - totalDeductions - totalExemptions;

            var stateTax = 0.0323 * indianaGrossIncome;
            var countyTax = 0.013825 * indianaGrossIncome;

            return stateTax + countyTax;

        }
    }
}
