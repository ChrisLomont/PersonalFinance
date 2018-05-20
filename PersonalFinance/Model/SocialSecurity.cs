using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lomont.PersonalFinance.Model
{
    public static class SocialSecurity
    {

        // compute social security
        // info from
        // https://www.thebalance.com/social-security-benefits-calculation-guide-2388927
        //
        // Step 1: Use your earnings history to calculate your Average Indexed Monthly Earnings (AIME).
        //      1.1 Start with a list of your earnings each year available online https://www.ssa.gov/myaccount/)
        //      1.2 Adjust for inflation using https://www.ssa.gov/oact/cola/AWI.html 
        //      1.3 take 35 highest, compute monthly avg (420 months in 35 years)
        // Step 2: Use your AIME to calculate your Primary Insurance Amount (PIA).
        // Step 3: Use your PIA and adjust it for the age you will begin benefits.
        //
        // nice calc at https://github.com/cfpb/retirement


        #region SSA Tables 

        // average wage index https://www.ssa.gov/oact/cola/AWI.html
        // (year, avg wage)
        static Dictionary<int, double> avgWageIndex = new Dictionary<int, double>
        {
            [1951] = 2799.16,
            [1952] = 2973.32,
            [1953] = 3139.44,
            [1954] = 3155.64,
            [1955] = 3301.44,
            [1956] = 3532.36,
            [1957] = 3641.72,
            [1958] = 3673.80,
            [1959] = 3855.80,
            [1960] = 4007.12,
            [1961] = 4086.76,
            [1962] = 4291.40,
            [1963] = 4396.64,
            [1964] = 4576.32,
            [1965] = 4658.72,
            [1966] = 4938.36,
            [1967] = 5213.44,
            [1968] = 5571.76,
            [1969] = 5893.76,
            [1970] = 6186.24,
            [1971] = 6497.08,
            [1972] = 7133.80,
            [1973] = 7580.16,
            [1974] = 8030.76,
            [1975] = 8630.92,
            [1976] = 9226.48,
            [1977] = 9779.44,
            [1978] = 10556.03,
            [1979] = 11479.46,
            [1980] = 12513.46,
            [1981] = 13773.10,
            [1982] = 14531.34,
            [1983] = 15239.24,
            [1984] = 16135.07,
            [1985] = 16822.51,
            [1986] = 17321.82,
            [1987] = 18426.51,
            [1988] = 19334.04,
            [1989] = 20099.55,
            [1990] = 21027.98,
            [1991] = 21811.60,
            [1992] = 22935.42,
            [1993] = 23132.67,
            [1994] = 23753.53,
            [1995] = 24705.66,
            [1996] = 25913.90,
            [1997] = 27426.00,
            [1998] = 28861.44,
            [1999] = 30469.84,
            [2000] = 32154.82,
            [2001] = 32921.92,
            [2002] = 33252.09,
            [2003] = 34064.95,
            [2004] = 35648.55,
            [2005] = 36952.94,
            [2006] = 38651.41,
            [2007] = 40405.48,
            [2008] = 41334.97,
            [2009] = 40711.61,
            [2010] = 41673.83,
            [2011] = 42979.61,
            [2012] = 44321.67,
            [2013] = 44888.16,
            [2014] = 46481.52,
            [2015] = 48098.63,
            [2016] = 48664.73
        };

        //  https://www.ssa.gov/oact/cola/cbb.html
        static Dictionary<int, double> maxWage = new Dictionary<int, double>
        {
            [1972] = 9000,
            [1973] = 10800,
            [1974] = 13200,
            [1975] = 14100,
            [1976] = 15300,
            [1977] = 16500,
            [1978] = 17700,
            [1979] = 22900,
            [1980] = 25900,
            [1981] = 29700,
            [1982] = 32400,
            [1983] = 35700,
            [1984] = 37800,
            [1985] = 39600,
            [1986] = 42000,
            [1987] = 43800,
            [1988] = 45000,
            [1989] = 48000,
            [1990] = 51300,
            [1991] = 53400,
            [1992] = 55500,
            [1993] = 57600,
            [1994] = 60600,
            [1995] = 61200,
            [1996] = 62700,
            [1997] = 65400,
            [1998] = 68400,
            [1999] = 72600,
            [2000] = 76200,
            [2001] = 80400,
            [2002] = 84900,
            [2003] = 87000,
            [2004] = 87900,
            [2005] = 90000,
            [2006] = 94200,
            [2007] = 97500,
            [2008] = 102000,
            [2009] = 106800,
            [2010] = 106800,
            [2011] = 106800,
            [2012] = 110100,
            [2013] = 113700,
            [2014] = 117000,
            [2015] = 118500,
            [2016] = 118500,
            [2017] = 127200,
            [2018] = 128700
        };

        //bend points from https://www.ssa.gov/OACT/COLA/bendpoints.html
        //eah year has 2 bend points for primary insurance amount, and 3 for maximum family benefit
        static Dictionary<int, double[]> bend_points = new Dictionary<int, double[]>
        {
            [1979] = new[] {180.0, 1085, 230, 332, 433},
            [1980] = new[] {194.0, 1171, 248, 358, 467},

            [1981] = new[] {211.0, 1274, 270, 390, 508},
            [1982] = new[] {230.0, 1388, 294, 425, 554},
            [1983] = new[] {254.0, 1528, 324, 468, 610},
            [1984] = new[] {267.0, 1612, 342, 493, 643},
            [1985] = new[] {280.0, 1691, 358, 517, 675},
            [1986] = new[] {297.0, 1790, 379, 548, 714},
            [1987] = new[] {310.0, 1866, 396, 571, 745},
            [1988] = new[] {319.0, 1922, 407, 588, 767},
            [1989] = new[] {339.0, 2044, 433, 626, 816},
            [1990] = new[] {356.0, 2145, 455, 656, 856},
            [1991] = new[] {370.0, 2230, 473, 682, 890},
            [1992] = new[] {387.0, 2333, 495, 714, 931},
            [1993] = new[] {401.0, 2420, 513, 740, 966},
            [1994] = new[] {422.0, 2545, 539, 779, 1016},
            [1995] = new[] {426.0, 2567, 544, 785, 1024},
            [1996] = new[] {437.0, 2635, 559, 806, 1052},
            [1997] = new[] {455.0, 2741, 581, 839, 1094},
            [1998] = new[] {477.0, 2875, 609, 880, 1147},
            [1999] = new[] {505.0, 3043, 645, 931, 1214},
            [2000] = new[] {531.0, 3202, 679, 980, 1278},
            [2001] = new[] {561.0, 3381, 717, 1034, 1349},
            [2002] = new[] {592.0, 3567, 756, 1092, 1424},
            [2003] = new[] {606.0, 3653, 774, 1118, 1458},
            [2004] = new[] {612.0, 3689, 782, 1129, 1472},
            [2005] = new[] {627.0, 3779, 801, 1156, 1508},
            [2006] = new[] {656.0, 3955, 838, 1210, 1578},
            [2007] = new[] {680.0, 4100, 869, 1255, 1636},
            [2008] = new[] {711.0, 4288, 909, 1312, 1711},
            [2009] = new[] {744.0, 4483, 950, 1372, 1789},
            [2010] = new[] {761.0, 4586, 972, 1403, 1830},
            [2011] = new[] {749.0, 4517, 957, 1382, 1803},
            [2012] = new[] {767.0, 4624, 980, 1415, 1845},
            [2013] = new[] {791.0, 4768, 1011, 1459, 1903},
            [2014] = new[] {816.0, 4917, 1042, 1505, 1962},
            [2015] = new[] {826.0, 4980, 1056, 1524, 1987},
            [2016] = new[] {856.0, 5157, 1093, 1578, 2058},
            [2017] = new[] {885.0, 5336, 1131, 1633, 2130},
            [2018] = new[] {896.0, 5399, 1145, 1652, 2155}
        };

        #endregion


        // Inflate value over [startYear inclusive,endYear inclusive]
        static double Inflate(int startYear, int endYear, double value, Func<int, double> inflation)
        {
            var iv = value;
            for (var year = startYear; year < endYear + 1; year++)
                iv *= 1 + inflation(year);
            return iv;
        }

        // add inflation adjusted wages to extend last known wage
        static double[] ExtendWages(double[] actualWages, int wageStartYear, int desiredLength,
            Func<int, double> inflation)
        {
            var wageCopy = new List<double>();
            wageCopy.AddRange(actualWages);
            var wage = actualWages.Last();
            var year = wageStartYear + actualWages.Length;
            while (wageCopy.Count < desiredLength)
            {
                wage *= 1 + inflation(year);
                wageCopy.Add(wage);
                year += 1;
            }
            return wageCopy.ToArray();
        }

        // compute Average Indexed Monthly Earnings
        static double ComputeAime(
            double[] actualWages, int wageStartYear, int wageStartAge,
            double retirementAge, Func<int, double> inflation)
        {

            //  extend actual wages till retirement age
            var desiredWageLength = (int) (retirementAge - wageStartAge);
            var extendedWages = ExtendWages(actualWages, wageStartYear, desiredWageLength, inflation);



            double GetWageIndex(int year)
            {
                if (avgWageIndex.ContainsKey(year))
                    return avgWageIndex[year];

                // not in table, inflate last as estimate
                var lastYear = avgWageIndex.Keys.Max();
                // max key
                var lastWage = avgWageIndex[lastYear];
                return Inflate(lastYear + 1, year, lastWage, inflation);
            }


            double GetMaxWage(int year)
            {
                if (year < 1937)
                    return 0;
                if (1937 <= year && year <= 1950)
                    return 3000;
                if (1951 <= year && year <= 1954)
                    return 3600;
                if (1955 <= year && year <= 1958)
                    return 4200;
                if (1959 <= year && year <= 1965)
                    return 4800;
                if (1966 <= year && year <= 1967)
                    return 6600;
                if (1968 <= year && year <= 1971)
                    return 7800;
                if (maxWage.ContainsKey(year))
                    return maxWage[year];

                var lastYear = maxWage.Keys.Max();
                var lastWage = maxWage[lastYear];

                return Inflate(lastYear + 1, year, lastWage, inflation);
            }


            //  SS rule - use age 60 index as base index
            var yearTurn60 = (60 - wageStartAge) + wageStartYear;

            // index to compare previous years to
            var indexingYearWages = GetWageIndex(yearTurn60);

            // store indexed wages here
            var indexedWages = new List<double>();
            for (var year = wageStartYear; year < wageStartYear + extendedWages.Length; ++year)
            {
                var yearlyWageIndex = GetWageIndex(year);
                var actualWage = Math.Min(extendedWages[year - wageStartYear], GetMaxWage(year));

                var indexedWage = Math.Round(indexingYearWages / yearlyWageIndex * actualWage);
                // print(indexing_year_wages,indexing_year_wages / wy, wy,actualWages[current_year-wageStartYear],indexed_wage)
                // print ("Indexed",indexed_wage, "wage year",wy,"indexe w",indexing_year_wages)
                // cap here from SSA tables max earnings      

                indexedWages.Add(indexedWage);
            }

            // sum at most 35 largest years
            indexedWages.Sort((a, b) => -a.CompareTo(b));
            if (indexedWages.Count > 35)
                indexedWages = indexedWages.Take(35).ToList();
            var indexedWagesSum = indexedWages.Sum();
            // print("Sum wages",indexed_wages_sum)
            // Average Indexed Monthly Earnings - 420 months in 35 years 
            var aime = indexedWagesSum / 420;

            // print("AIME",aime)

            return aime;
        }


        static void GetBendPoints(int age62Year, double[] bendPoints, Func<int, double> inflation)
        {
            bendPoints[2] = 0;
            if (bend_points.Keys.Contains(age62Year))
            {
                var item = bend_points[age62Year];
                bendPoints[0] = item[0];
                bendPoints[1] = item[1];
            }
//  inflate highest
            var year = bend_points.Keys.Max();
            var item1 = bend_points[year];
            year += 1;
            bendPoints[0] = Inflate(year, age62Year, item1[0], inflation);
            bendPoints[1] = Inflate(year, age62Year, item1[1], inflation);
        }

        static double ComputePia(double aime, int age62Year, Func<int, double> inflation)
        {
            // compute Primary Insurance Amount, amount received if retire at Full Retirement Age (FRA)
            //  bend points, look up for a given year
            //  each is a % and $ amount
            //  take this % of the amount, then next, amount, then next. Last % has 0 which means take all existing
            //  bends = {(0.90,826),(0.32,4980),(0.15,0)} 
            var bends1 = new double[3];
            GetBendPoints(age62Year, bends1, inflation);
            var bends2 = new[] {0.90, 0.32, 0.15};
            var pia = 0.0;
            var aimeLeft = aime;
            for (var i = 0; i < 3; ++i)
            {
                var a = bends1[i];
                var p = bends2[i];
                pia += p * Math.Min(a, aimeLeft);
                aimeLeft = Math.Max(0, aimeLeft - a);
            }
            //  round to next lowest dime
            pia = (int) (pia * 10) / 10.0;
            return pia;
        }

        // compute full retirement age
        static double ComputeFra(int birthYear)

        {
            if (birthYear < 1938)
                return 65.0;
            if (birthYear < 1943)
                return 65 + (birthYear - 1937) * 2.0 / 12;
            if (1943 <= birthYear && birthYear < 1955)
                return 66.0;
            if (birthYear < 1960)
                return 66 + (birthYear - 1954) * 2.0 / 12;
            return 67.0;
        }

        static double ComputeReduction(int retirementAge, int birthYear)
        {
            var fra = ComputeFra(birthYear);

            var monthsShort = Math.Max((fra - retirementAge) * 12, 0);
            // 5/9 of % per month up to 36, 5/12 of % per month over 36
            var reduction = 0.0;
            if (monthsShort <= 36)
                reduction = monthsShort * 5.0 / 9 * 0.01;
            else if (monthsShort > 36)
                reduction = 36 * 5.0 / 9 * 0.01 + (monthsShort - 36) * 5.0 / 12 * 0.01;

            if (reduction > 0)
                return 1 - reduction;

            // if born past 1943, 2/3 of % each month over, to max of ???:
            var monthsOver = 0.0;
            if (birthYear >= 1943 && monthsShort <= 0)
                monthsOver = retirementAge - fra; // max!
            var increase = monthsOver * 2 / 3 * 0.01;

            return 1 + increase;
        }

        public static (double ss, double aime, double pia, double reduction)
            ComputeSocialSecurityParams(
                double[] actualWages,
                int wageStartYear,
                int startAge,
                int retirementAge,
                Func<int, double> inflation
            )
        {
            // compute ss, aime, pia, reduction
            // print(wageStartYear,wageStartAge)
            // print (actualWages)

            var aime = ComputeAime(actualWages, wageStartYear, startAge, retirementAge, inflation);
            // rint("AIME",aime)


            var age62Year = 62 - startAge + wageStartYear;
            var pia = ComputePia(aime, age62Year, inflation);
            // print("PIA",pia)

            var birthYear = age62Year - 62;
            var reduction = ComputeReduction(retirementAge, birthYear);
            // print(reduction)

            var ssMonthly = pia * reduction;

            //  todo - is there some max by year amount allowed to pay out?
            return (ssMonthly, aime, pia, reduction);
        }

        /// <summary>
        /// Compute social security monthly payout
        /// </summary>
        /// <param type="actualWages"></param>
        /// <param type="wageStartYear"></param>
        /// <param type="startAge"></param>
        /// <param type="retirementAge"></param>
        /// <param type="inflation"></param>
        /// <returns></returns>
        public static double ComputeSocialSecurity(
                double[] actualWages,
                int wageStartYear,
                int startAge,
                int retirementAge,
                Func<int, double> inflation
            )
        {
            var (ssMonthly, aime, pia, reduction) = ComputeSocialSecurityParams(
                actualWages,
                wageStartYear,
                startAge,
                retirementAge,
                inflation
                );
            return ssMonthly;
        }

        public static void Test()
        {
            // simple inflation
            double Inflation(int y) => 0.02;

            if (false) //  test data from site https://www.thebalance.com/social-security-benefits-calculation-guide-2388927

            {
                // ans 1455
                // your wages from SS site
                var actualWages = new double[]
                {
                    1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 10000, 11000, 18000, 20000, 21000, 22000,
                    23000, 24000, 25000,
                    25000, 25000, 27000, 29000, 30000, 36000, 37000, 38000, 39000, 40000, 41000, 42000, 40000, 40000,
                    40000, 43000,
                    45000, 46000, 48000, 50000, 44000, 44000, 46000, 48000, 45000, 45000
                };
                // first recorded wage is for what year?    
                var wageStartYear = 1971;
                // age of person at first year
                var ageAtStartYear = 18;
                // desired retirement age - first year of no income
                var retirementAge = 62;
                // can be a min of 62, max of 70 (after which useless - no more gain)
                var ssMonthly = ComputeSocialSecurity(actualWages, wageStartYear, ageAtStartYear, retirementAge, Inflation);
                Debug.WriteLine($"SS is {ssMonthly}");
            }
            else
            {
                // chris data
                // from SS site, my FRA is 2253, from me, 2867 using data through 2016, (has my growth in it)
                // 

                // FRA 3213 using estimated 2017, 2018 data, 2% growth
                // retirement 70, 3278 using estimated 2017, 2018 data, 2% growth

                /*
                 * Chris SS data 1986-2016 (missing 2001,2002, incorrect!)
                    2587,1600,809,572,3355,1067,24967,32764,59654,22301,17759,13120,20250,13824,17710,0,0,44904,
                    73747,80713,84732,97500,102000,104621,106800,106800,105314,17930,43044,29631,42990
                */


                // your wages from SS site https://www.ssa.gov/myaccount/
                var actualWages = new double[]
                {
                    2587, // 1986, age 18
                    1600,809,572,3355,1067,24967,32764,59654,22301,17759,13120,20250,13824,17710,0,0,44904,
                    73747,80713,84732,97500,102000,104621,106800,106800,105314,17930,43044,29631,
                    42990, // 2016
                    30000, // 2017
                    125000 // 2018
                };

                // first recorded wage is for what year?    
                var wageStartYear = 1986;
                // age of person at first year
                var ageAtStartYear = 18;
                // desired retirement age - first year of no income
                var retirementAge = 70;
                // can be a min of 62, max of 70 (after which useless - no more gain)
                var ssMonthly = ComputeSocialSecurity(actualWages, wageStartYear, ageAtStartYear, retirementAge, Inflation);
                Debug.WriteLine($"SS is {ssMonthly}");
            }
        }
    }
}
