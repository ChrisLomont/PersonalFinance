using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lomont.PersonalFinance.Model.Item;

namespace Lomont.PersonalFinance.Model.Legal
{
    [Serializable]
    public enum FilingStatus
    {
        Single,
        MarriedJointly,
        MarriedSeparately,
        HeadOfHousehold,
        WidowerWithDependentChild
    }
    [Serializable]
    public class Form1040
    {

        public FilingStatus FilingStatus { get; set;  } = FilingStatus.Single;

        /// <summary>
        /// Yourself is 1, spouse if jointly is 2, each dependent is one more
        /// </summary>
        public int Exemptions { get; set; } = 1;

        /// <summary>
        /// Wage input
        /// </summary>
        public double Wages { get; set;  }

        public double TaxableInterest { get; set; }

        /// <summary>
        /// Note qualified dividends don't get taxed
        /// </summary>
        public double OrdinaryDividends { get; set; }

        /// <summary>
        /// Attach Schedule C or C-EZ
        /// </summary>
        public double BusinessIncomeOrLoss { get; set; }

        public double CapitalGainOrLoss { get; set; }

        public double IraDistributionsTaxableAmount { get; set; }
        public double PensionAndAnnuitiesTaxableAmount { get; set; }

        public double SocialSecurityTaxableAmount { get; set; }

        public double OtherIncomeTaxableAmount { get; set; }

        /// <summary>
        /// Amount paid into an IRA
        /// </summary>
        public double IraDeduction { get; set;  }


        /// <summary>
        ///AGI, computed internally
        /// </summary>
        public double AdjustedGrossIncome { get; private set;  }


        static double[] taxBackets2018 =
        {// rate, single threshold, married joint threshold
            0.10,   9525,  19050, 
            0.12,  38799,  77400,
            0.22,  82500, 165000, 
            0.24, 157500, 315000,
            0.32, 200000, 400000, 
            0.35, 500000, 600000, 
            0.37, Double.MaxValue, Double.MaxValue

        };

        /// <summary>
        /// Compute federal tax
        /// </summary>
        /// <param name="log"></param>
        /// <param name="compoundInflation"></param>
        /// <returns></returns>
        public double Compute(DDataTable log, double compoundInflation)
        {   // see http://taxhow.net/articles/1040-step-by-step-guide
            // todo - Alternative Minimum Tax

            var totalIncome = 0.0;
            totalIncome += Wages;
            totalIncome += TaxableInterest;
            totalIncome += OrdinaryDividends;
            totalIncome += BusinessIncomeOrLoss;
            totalIncome += CapitalGainOrLoss;
            totalIncome += IraDistributionsTaxableAmount;
            totalIncome += PensionAndAnnuitiesTaxableAmount;
            totalIncome += SocialSecurityTaxableAmount;
            totalIncome += OtherIncomeTaxableAmount;

            AdjustedGrossIncome = totalIncome - IraDeduction; // todo - other things here

            // todo - implement itemized deductions
            // todo - these are 2018+ sizes
            var standardDeduction = compoundInflation * 12000;
            if (FilingStatus == FilingStatus.MarriedJointly)
                standardDeduction = compoundInflation * 24000;

            var exemptionDeduction = 0.0;
            // personal exemption removed in 2018
            //if (adjustedGrossIncome < compoundInflation * 155650) // todo - also tapers off
            //    exemptionDeduction = Exemptions * compoundInflation * 4150;

            var taxableIncome = AdjustedGrossIncome - standardDeduction - exemptionDeduction;

            var totalTax = ComputeTax(taxableIncome, taxBackets2018, compoundInflation);

            // todo - social security tax? take before agi?

            return totalTax;
        }

        double ComputeTax(double taxableIncome, double[] brackets, double compoundInflation)
        {
            var bracketIndex = 0;
            var bottomRange = 0.0;
            var totalTax = 0.0;
            var offset = FilingStatus == FilingStatus.Single ? 1 : 2;
            while (taxableIncome > 0)
            {
                var rate = brackets[bracketIndex * 3];
                var range = compoundInflation * (brackets[bracketIndex*3 + offset] - bottomRange);

                var incomeInRange = Math.Min(range,taxableIncome);
                taxableIncome -= incomeInRange;
                totalTax += rate * incomeInRange;

                bottomRange = brackets[bracketIndex * 3 + offset];
                ++bracketIndex;
            }
            return totalTax;
        }
    }
}
