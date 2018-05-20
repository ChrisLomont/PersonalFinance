using Lomont.PersonalFinance.Model.Legal;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lomont.PersonalFinance.Model.Item;

namespace Lomont.PersonalFinance.Model
{

    public enum AssetType
    {
        // assets
        Cash,
        Taxable,
        Untaxable,
        Retirement401K,
        House,
        Items, // other items of value: gold, jewelry, art, etc.

        //debts
        Mortgage, 
        Loan
    }

    /// <summary>
    /// Represent a single stream of flows - 
    /// has income, expenses, assets, debts
    /// basically a person
    /// </summary>
    [Serializable]
    public class Actor
    {
        // RateType of account holder
        public string Name;

        // static
        public double Age;
        public double RetirementAge;     // stops income
        public double Take401KAge;       // must be after 59.5 else penalty, must be before 70.5

        public double SocialSecurityAge; // when start taking Social Security
        // your wages from SS site https://www.ssa.gov/myaccount/
        public double [] YearlyWages;
        // first recorded wage is for what year?    
        public int WageStartYear;
        // age of person at first year
        public int AgeAtStartYear;

        // income
        public double AnnualWageIncome;
        public double MonthlySocialSecurityIncome; // value of SS income whether taken or not
        public double AnnualOtherTaxableIncome;    // income from other sources
        public double AnnualOtherUntaxableIncome;  // untaxable income, such as taking out from Roth

        // expenses
        public double Annual401KContribution;
        public double MonthlyExpenses;
        public double AnnualOtherExpenses;

        // assets
        public double DesiredCashOnHand;
        public List<Asset> Assets;

        // debts
        public List<Debt> Debts; // mortgage, loans, credit cards, etc.

        Taxation tax = new Taxation();

        (double interest, double payments) ProcessLoan(Debt loan)
        {
            var interest = loan.Principal * loan.AnnualRate;
            loan.Principal += interest;
            var amtPaid = Math.Min(loan.MonthlyPayment * 12, loan.Principal);
            loan.Principal -= amtPaid;

            return (interest, amtPaid);
        }

        // return interest
        double ProcessAsset(Asset asset, double rate)
        {
            var interest = asset.Value * rate;
            asset.Value += interest;
            return interest;

        }

        public Asset GetAsset(AssetType type)
        {
            foreach (var asset in Assets)
                if (asset.AssetType == type)
                    return asset;
            return null;
        }

        public Debt GetDebt(AssetType type)
        {
            foreach (var d in Debts)
                if (d.DebtType == type)
                    return d;
            return null;
        }

        class Income
        {
            public double taxableIncome = 0, untaxableIncome = 0;
            public double totalIncome => taxableIncome + untaxableIncome;
        }

        class Expenses
        {
            public double totalExpenses;
            public double untaxableAmount;
        }


        double GetAnnual401KContribution(LocalState ls)
        {
            return !ls.retired ? Annual401KContribution : 0;
        }

        Income ComputeIncome(LocalState ls)
        {
            // income - track taxable and untaxable
            // todo - add roth and other IRA
            var income = new Income();

            var annualWages = !ls.retired ? AnnualWageIncome : 0;
            income.taxableIncome += annualWages;
            ls.log.AddData(annualWages, Col.Make(ColType.Income, "Wage", Name));

            var annualSocSecIncome = ls.collectingSocialSecurity ? MonthlySocialSecurityIncome * 12 : 0;
            income.taxableIncome += annualSocSecIncome;
            ls.log.AddData(annualSocSecIncome, Col.Make(ColType.Income, "Soc Sec", Name));

            var ret401 = GetAsset(AssetType.Retirement401K);
            var annual401KIncome = (Age < 86 && ls.taking401k) ? ret401.Value / (86 - Age) : 0; // todo - from tables? 86 age?
            income.taxableIncome += annual401KIncome;
            ret401.Value -= annual401KIncome;
            ls.log.AddData(annual401KIncome, Col.Make(ColType.Income, "401k", Name));


            var annual401KContrib = GetAnnual401KContribution(ls);
            income.taxableIncome -= annual401KContrib;
            income.untaxableIncome -= annual401KContrib;

            // todo - investment taxes
            var taxableInterest = 0.0;
            foreach (var asset in Assets)
                taxableInterest = ProcessAsset(asset,
                    StateUtils.GetRate(ls.state.Rates, asset.RateName, (int) ls.state.CurrentYear));
            income.taxableIncome += taxableInterest;
            ls.log.AddData(AnnualOtherTaxableIncome, Col.Make(ColType.Income, "investment income", Name));

            income.taxableIncome += AnnualOtherTaxableIncome;
            ls.log.AddData(AnnualOtherTaxableIncome, Col.Make(ColType.Income, "other taxable", Name));

            income.untaxableIncome += AnnualOtherUntaxableIncome;
            ls.log.AddData(AnnualOtherUntaxableIncome, Col.Make(ColType.Income, "untaxable", Name));

            ls.log.AddData(income.totalIncome, Col.Make(ColType.Income, "Total", Name));

            // inflation of income variables
            AnnualWageIncome *= 1 + ls.inflation;
            MonthlySocialSecurityIncome *= 1 + ls.inflation;

            return income;
        }

        Expenses ComputeExpenses(LocalState ls)
        {
            var expenses = new Expenses();

            // expenses (and some debt)
            var ret401 = GetAsset(AssetType.Retirement401K);
            var annual401KContrib = GetAnnual401KContribution(ls);
            ret401.Value += annual401KContrib;
            expenses.totalExpenses += annual401KContrib;
            
            ls.log.AddData(annual401KContrib, Col.Make(ColType.Expense, "401k", Name));

            expenses.totalExpenses += 12 * MonthlyExpenses;
            ls.log.AddData(MonthlyExpenses * 12, Col.Make(ColType.Expense, "Monthly expenses", Name));
            expenses.totalExpenses += AnnualOtherExpenses;
            ls.log.AddData(AnnualOtherExpenses, Col.Make(ColType.Expense, "Annual other", Name));

            // pay debts, tax implications;
            foreach (var d in Debts)
            {
                var (interest1, payments1) = ProcessLoan(d);
                expenses.totalExpenses += payments1;
                //if (d.RateType == AssetType.Mortgage)
                //    taxableIncome -= interest1; // todo - log this, do correctly
                ls.log.AddData(payments1, Col.Make(ColType.Expense, d.Name, Name));
            }

            // inflation of expense variables
            MonthlyExpenses *= 1 + ls.inflation;
            Annual401KContribution *= 1 + ls.inflation;
            AnnualOtherExpenses *= 1 + ls.inflation;

            return expenses;
        }

        /// <summary>
        /// Compute taxes from income and expenses, update expenses
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="income"></param>
        /// <param name="expenses"></param>
        private void ComputeTaxation(LocalState ls, Income income, Expenses expenses)
        {
            // todo - track all through tax object

            // todo - amount nonneg
            income.taxableIncome -= expenses.untaxableAmount;
            income.untaxableIncome += expenses.untaxableAmount;

            // taxes 
            // todo - get income, remove pretax, add in capital gains, use tax rules
            // todo - mortgage deduction

            // todo - place thes throughout calculation
            var f = tax.Federal;
            f.FilingStatus = FilingStatus.Single;
            f.Exemptions = 1;
            f.BusinessIncomeOrLoss = 0;
            f.CapitalGainOrLoss = 0;
            f.IraDeduction = 0;
            f.IraDistributionsTaxableAmount = 0;
            f.OrdinaryDividends = 0;
            f.OtherIncomeTaxableAmount = 0;
            f.PensionAndAnnuitiesTaxableAmount = 0;
            f.SocialSecurityTaxableAmount = 0;
            f.TaxableInterest = 0;
            f.Wages = income.taxableIncome;

            var s = tax.State;
            s.HomeownerPropertyTax = 0;
            s.PeopleOver65 = 0;
            s.TaxableSocialSecurityBenefits = 0;

            var annualTaxes = tax.ComputeTax(ls.log, ls.totalInflation);


            // var annualTaxes = income.taxableIncome * 0.35; // todo - make smarter
            expenses.totalExpenses += annualTaxes;
            ls.log.AddData(annualTaxes, Col.Make(ColType.Expense, "Taxes", Name));
            ls.log.AddData(annualTaxes/income.taxableIncome, Col.Make(ColType.Expense, "Tax rate", Name));
            ls.log.AddData(expenses.totalExpenses, Col.Make(ColType.Expense, "Total", Name));
        }

        // try to make asserts meet min value of 0, except first which needs min value of minVal
        // if not possible, return true
        bool CashFlow(double firstDesiredVal, params Asset[] assets1)
        {
            var assets = new List<Asset>();
            foreach (var v in assets1)
                if (v != null)
                    assets.Add(v);
            if (firstDesiredVal > assets[0].Value)
            {
                var need = firstDesiredVal - assets[0].Value;
                assets[0].Value += need;
                assets[1].Value -= need;
            }
            if (firstDesiredVal < assets[0].Value)
            {
                var excess = assets[0].Value - firstDesiredVal;
                assets[0].Value -= excess;
                assets[1].Value += excess;
            }
            var i = 1;
            while (assets[i].Value < 0 && i < assets.Count-1)
            {
                var loss = -assets[i].Value;
                assets[i].Value = 0;
                assets[i + 1].Value -= loss;
                ++i;
            }
            return assets.Last().Value < 0;
        }


        /// <summary>
        /// Given some cash (positive or negative), manage asset allocation
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="netIncome"></param>
        (bool bankrupt, double totalAssets) ManageAssets(LocalState ls, double netIncome)
        {
            // manage assets with net income
            // if out of assets set all to zero, set bankrupt flag, stop processing
            var cashOnHand = GetAsset(AssetType.Cash);
            cashOnHand.Value += netIncome;

            // now flow downwards to make all positive, in this order
            // if no value, bankrupt
            var bankrupt = CashFlow(
                DesiredCashOnHand,
                GetAsset(AssetType.Cash),
                GetAsset(AssetType.Taxable),
                GetAsset(AssetType.Items)
                //                GetAsset(AssetType.House)
                );

            DesiredCashOnHand *= 1 + ls.inflation;

            // tally assets
            var totalAssets = 0.0;
            foreach (var asset in Assets)
            {
                totalAssets += asset.Value;
                
                ls.log.AddData(asset.Value, Col.Make(ColType.Asset, asset.Name, Name));
            }
            ls.log.AddData(totalAssets, Col.Make(ColType.Asset, "Total", Name));

            return (bankrupt, totalAssets);
        }

        double ManageDebts(LocalState ls)
        {
            // Debts
            var totalDebts = 0.0;
            foreach (var d in Debts)
            {
                totalDebts += d.Principal;
                
                ls.log.AddData(d.Principal, Col.Make(ColType.Debt, d.Name, Name));
            }
            ls.log.AddData(totalDebts, Col.Make(ColType.Debt, "Total", Name));
            return totalDebts;
        }

        // todo - implement asset allocation in retirement places: 120 - age in stocks, rest in bonds

        class LocalState
        {
            public bool retired;
            public bool collectingSocialSecurity;
            public bool taking401k;
            public DDataTable log;
            public double inflation;
            public State state;
            public double totalInflation;
        }
        
        // step one year
        // return true if not bankrupt
        public bool Step(DDataTable log, double inflation, State state)
        {
            var inflator = StateUtils.GetRateItem(state.Rates, State.InflationName);

            // some nice flags to have handy
            var ls = new LocalState
            {
                retired = Age >= RetirementAge,
                collectingSocialSecurity = Age >= SocialSecurityAge, // todo - make a choice
                taking401k = Age >= Take401KAge,
                log = log,
                inflation = inflation,
                state = state,
                totalInflation = StateUtils.TotalRate((int)state.StartYear, (int)state.CurrentYear, inflator)
            };


            // logging
            log.AddData(Age, Col.Make(ColType.Info, "Age", Name));

            // compute and log income items
            var income = ComputeIncome(ls);

            // compute and log expenses
            var expenses = ComputeExpenses(ls);

            ComputeTaxation(ls, income, expenses);

            // net flow amount (post tax)
            var netIncome = income.totalIncome - expenses.totalExpenses;
            log.AddData(netIncome, Col.Make(ColType.Info, "Net cash flow", Name));

            // Update assets with income gain or loss
            var (bankrupt, totalAssets) = ManageAssets(ls, netIncome);

            var totalDebts = ManageDebts(ls);

            // net worth
            var netWorth = totalAssets - totalDebts;
            log.AddData(netWorth, Col.Make(ColType.Info, "Net Worth", Name));

            if (bankrupt)
                netWorth = totalAssets = 0;

            return true; // todo - fill in all values
            return !bankrupt;
        }


        /// <summary>
        /// Used elsewhere to prioritize things
        /// </summary>
        public static string FinalValueText = "Net Worth";
    }
}
