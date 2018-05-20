﻿using System;
using Lomont.PersonalFinance.Model.Distributions;

namespace Lomont.PersonalFinance.Model.Economy
{
    static class InvestmentTables
    { // from http://pages.stern.nyu.edu/~adamodar/New_Home_Page/datafile/histretSP.html

        /// <summary>
        /// S&P500 returns 1928 to 2016
        /// </summary>
        public static double[] sp500Returns1928To2016 =
        {
43.81,
-8.30,
-25.12,
-43.84,
-8.64,
49.98,
-1.19,
46.74,
31.94,
-35.34,
29.28,
-1.10,
-10.67,
-12.77,
19.17,
25.06,
19.03,
35.82,
-8.43,
5.20,
5.70,
18.30,
30.81,
23.68,
18.15,
-1.21,
52.56,
32.60,
7.44,
-10.46,
43.72,
12.06,
0.34,
26.64,
-8.81,
22.61,
16.42,
12.40,
-9.97,
23.80,
10.81,
-8.24,
3.56,
14.22,
18.76,
-14.31,
-25.90,
37.00,
23.83,
-6.98,
6.51,
18.52,
31.74,
-4.70,
20.42,
22.34,
6.15,
31.24,
18.49,
5.81,
16.54,
31.48,
-3.06,
30.23,
7.49,
9.97,
1.33,
37.20,
22.68,
33.10,
28.34,
20.89,
-9.03,
-11.85,
-21.97,
28.36,
10.74,
4.83,
15.61,
5.48,
-36.55,
25.94,
14.82,
2.10,
15.89,
32.15,
13.52,
1.36,
11.74,


        };

        public static double[] threeMonthTBillReturns1928To2016 =
        {
3.08,
3.16,
4.55,
2.31,
1.07,
0.96,
0.32,
0.18,
0.17,
0.30,
0.08,
0.04,
0.03,
0.08,
0.34,
0.38,
0.38,
0.38,
0.38,
0.57,
1.02,
1.10,
1.17,
1.48,
1.67,
1.89,
0.96,
1.66,
2.56,
3.23,
1.78,
3.26,
3.05,
2.27,
2.78,
3.11,
3.51,
3.90,
4.84,
4.33,
5.26,
6.56,
6.69,
4.54,
3.95,
6.73,
7.78,
5.99,
4.97,
5.13,
6.93,
9.94,
11.22,
14.30,
11.01,
8.45,
9.61,
7.49,
6.04,
5.72,
6.45,
8.11,
7.55,
5.61,
3.41,
2.98,
3.99,
5.52,
5.02,
5.05,
4.73,
4.51,
5.76,
3.67,
1.66,
1.03,
1.23,
3.01,
4.68,
4.64,
1.59,
0.14,
0.13,
0.03,
0.05,
0.07,
0.05,
0.21,
0.51,
        };

        public static double[] tenYearBondReturns1928To2016 =
        {
0.84,
4.20,
4.54,
-2.56,
8.79,
1.86,
7.96,
4.47,
5.02,
1.38,
4.21,
4.41,
5.40,
-2.02,
2.29,
2.49,
2.58,
3.80,
3.13,
0.92,
1.95,
4.66,
0.43,
-0.30,
2.27,
4.14,
3.29,
-1.34,
-2.26,
6.80,
-2.10,
-2.65,
11.64,
2.06,
5.69,
1.68,
3.73,
0.72,
2.91,
-1.58,
3.27,
-5.01,
16.75,
9.79,
2.82,
3.66,
1.99,
3.61,
15.98,
1.29,
-0.78,
0.67,
-2.99,
8.20,
32.81,
3.20,
13.73,
25.71,
24.28,
-4.96,
8.22,
17.69,
6.24,
15.00,
9.36,
14.21,
-8.04,
23.48,
1.43,
9.94,
14.92,
-8.25,
16.66,
5.57,
15.12,
0.38,
4.49,
2.87,
1.96,
10.21,
20.10,
-11.12,
8.46,
16.04,
2.97,
-9.10,
10.75,
1.28,
0.69,


        };

        public static double SampleSeries(int startYear, int endYear, LocalRandom random, double[] series)
        {
            if (startYear < 1928 || 2016 < startYear || endYear < 1928 || 2016 < endYear || endYear < startYear)
                throw new ArgumentException("Start year and end year must be in 1928-2016");
            var len = endYear - startYear + 1;
            var index = random.Next(len) + (startYear-1928);
            return series[index]/100.0;
        }
    }
}
