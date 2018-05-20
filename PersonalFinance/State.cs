// #define INTERNAL_STATE
using System;
using System.Collections.Generic;
using System.Linq;
using Lomont.PersonalFinance.Model.Distributions;
using Lomont.PersonalFinance.Model.Economy;
using Lomont.PersonalFinance.Model.Item;
using Lomont.PersonalFinance.Model.Legal;


namespace Lomont.PersonalFinance.Model
{


    [Serializable]

    public class State
    {
        public static string InflationName = "Inflation rate";
        public static string StockReturnsName = "Stockmarket rate";
        public static string NoGrowthName = "No growth rate";


        public string Version = "1.0"; // file/code version

        [Variable(2017,2100,1, "Start year")]
        public double StartYear = 2017;
        public double CurrentYear = 2017;
        [Variable(1, 100, 1, "Number of years")]
        public double NumYears = 60;

        public List<Rate> Rates = new List<Rate>
        {
#if INTERNAL_STATE
            // mean CAGR of S&P 500 1928-2015
            // 19.7% std dev
            // Uses Laplace dist
            // https://sixfigureinvesting.com/2016/03/modeling-stock-market-returns-with-laplace-distribution-instead-of-normal/
            // normal https://www.ifa.com/articles/does_this_machine_simulate_market_returns/
            // new Rate(RateType.StockModel, 0.07,0.00, new StockModel()), // todo - this goes crazy 0.095, 0.197),

            new Rate(StockReturnsName,  RateDistribution.NormalDistribution, 0.095, 0.197), 
            
            // https://www.bogleheads.org/forum/viewtopic.php?t=147583
            new Rate(InflationName, RateDistribution.InflationModel, 0.03, 0.013),
            new Rate(NoGrowthName,RateDistribution.NormalDistribution, 0.0, 0.0),// no growth

            // todo - model mixed portfolios (120-age rule?)
            // https://personal.vanguard.com/us/insights/saving-investing/model-portfolio-allocations
#endif
        };

        public List<Actor> Actors = new List<Actor>
        {
#if INTERNAL_STATE

            new Actor
            {
                // basic data
                Name = "Chris",
                Age = 49,
                RetirementAge = 70,
                Take401KAge = 70,


                // Social security section
                SocialSecurityAge = 70, // SS is $3589/mo if 70, $3343 if 69, 
                // your wages from SS site https://www.ssa.gov/myaccount/
                YearlyWages = new double[]{
                    2587, // 1986, age 18
                    1600,809,572,3355,1067,24967,32764,59654,22301,17759,13120,20250,13824,17710,0,0,44904,
                    73747,80713,84732,97500,102000,104621,106800,106800,105314,17930,43044,29631,
                    42990, // 2016
                    30000, // 2017 - guess
                    125000 // 2018 - guess
                },
                WageStartYear = 1986,
                AgeAtStartYear = 18,


                // income
                AnnualWageIncome = 125000,
                MonthlySocialSecurityIncome = 3685,
                AnnualOtherTaxableIncome = 2500,
                AnnualOtherUntaxableIncome = 0,
                // todo - add 401k matching, % max, etc, rules

                // expenses
                Annual401KContribution = 18000,
                MonthlyExpenses = 1900,
                AnnualOtherExpenses = 10000,

                // assets
                DesiredCashOnHand = 5000,
                Assets = new List<Asset>
                {
                    new Asset("Cash", AssetType.Cash)
                    {
                        Value = 20000,
                        RateName = NoGrowthName
                    },
                    new Asset("Taxable Investments", AssetType.Taxable)
                    { // todo - split into stocks and bonds for yields
                        Value = 0,
                        RateName = StockReturnsName
                    },
                    new Asset("Retirement401K", AssetType.Retirement401K)
                    {
                        Value = 200000,
                        RateName = StockReturnsName
                    },
                    new Asset("Item Asset", AssetType.Items)
                    {
                        Value = 60000,
                        RateName = NoGrowthName
                    },
                    new Asset("House", AssetType.House)
                    {
                        Value = 500000, // todo - link to mortgage? this is likely larger - make mortgage section
                        RateName=InflationName // todo - correct assumption?
                    }
                },
                Debts = new List<Debt>
                {
                    new Debt("Mortgage", AssetType.Mortgage)
                    {
                        Principal = 200000,
                        AnnualRate = 0.05,
                        Payments = 30 * 12,
                        MonthlyPayment = 1073.64
                    },
                }
            },
#if true
            new Actor
            {
                // basic data
                Name = "Stacie",
                Age = 42,
                RetirementAge = 70,
                Take401KAge = 70,

                // Social security section
                SocialSecurityAge = 70, // SS is todo $3589/mo if 70, $1848 if 67, 
                // your wages from SS site https://www.ssa.gov/myaccount/
                YearlyWages = new double[]{
                2160, 3620, 5743, 9125, 13156, 14895, 22678, 20182, 21747, 20916, 21773,
                24008, 29040, 30546, 34879, 34095, 32699, 36940, 37408, 34470, 33440,
                43743, 28120, 21337, 34443, 52127
                },
                WageStartYear = 1991,
                AgeAtStartYear = 16,


                // income
                AnnualWageIncome = 52127,
                MonthlySocialSecurityIncome = 2400,
                AnnualOtherTaxableIncome = 0,
                AnnualOtherUntaxableIncome = 0,

                // expenses
                Annual401KContribution = 0.07*52127,
                MonthlyExpenses = 2000,
                AnnualOtherExpenses = 0,

                // assets
                DesiredCashOnHand = 2000, // todo - inflate
                Assets = new List<Asset>
                {
                    new Asset("Cash", AssetType.Cash)
                    {
                        Value = 30000,
                        RateName = NoGrowthName
                    },
                    new Asset("Taxable Investments", AssetType.Taxable)
                    { // todo - split into stocks and bonds for yields
                        Value = 0,
                        RateName = StockReturnsName
                    },
                    new Asset("Retirement401K", AssetType.Retirement401K)
                    {
                        Value = 120000,
                        RateName = StockReturnsName
                    },
                },
                Debts = new List<Debt>
                {
                    // new Debt("401k Loan", ItemType.Loan)
                    // { todo - pay to stacie
                    //     Principal = 3800,
                    //     AnnualRate = 0.0625,
                    //     Payments = 100,
                    //     MonthlyPayment = 196
                    // },
                    new Debt("Car Loan", AssetType.Loan)
                    {
                        Principal = 13000,
                        AnnualRate = 0.11,
                        Payments = 100,
                        MonthlyPayment = 285
                    },
                    new Debt("Loans1", AssetType.Loan)
                    { 
                        Principal = 15000,
                        AnnualRate = 0.035,
                        Payments = 100,
                        MonthlyPayment = 22
                    },
                    new Debt("Loans2", AssetType.Loan)
                    { // todo - paid after 10?
                        Principal = 48000,
                        AnnualRate = 0.065,
                        Payments = 100,
                        MonthlyPayment = 500 // is actually 48
                    },
                    new Debt("CreditCards", AssetType.Loan)
                    {
                        Principal = 10000,
                        AnnualRate = 0.16,
                        Payments = 100,
                        MonthlyPayment = 300
                    }
                }
            }
#endif

#endif
        };


    }
}
