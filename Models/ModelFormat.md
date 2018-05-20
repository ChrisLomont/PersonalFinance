# Financial planner model format
version 1.0

Since the JSON doesn't let there be comments, here are what the parameters mean and how to fill them in.


| Parameter     | Meaning       |
| ------------- |---------------|
| StartYear     | Start of sim  |
| CurrentYear   | ???           |
| NumYears      | Length of sim |
| Rates         | a table of rates and models (1) |
| Actors        | people/entities in the model (2) |

### (1) Rates

The simulation has various interest/growth rates with associated models. The default has a stockmarket return, an inflation rate, and a no-growth rate, used in various places. 

| Parameter         | Meaning       |
| -------------     |---------------|
| Name              | a name for the rate      |
| RateDistribution  | the distribution model   |
| Parameters        | a list of double for the model parameters |


### (2) Actors
A actor is a person (or some other entity) that has assets, inflows, outflows, debts, etc.

| Parameter                     | Meaning                                    |
| -------------                 |---------------                             |
| Name                          | a name for the actor                       |
| Age                           | age in start year                          |
| RetirementAge                 | planned retirement age                     |
| Take401KAge                   | planned age to start taking 401k           |
| SocialSecurityAge             | planned age to start social security age   |
| YearlyWages                   | wages from social security site (3)        |
| WageStartYear                 | start year for the SS wages (3)            |
| AgeAtStartYear                | start age for SS wages (3)                 |
| AnnualWageIncome              | Current annual wages                       |
| MonthlySocialSecurityIncome   | expected social security income(4)         |
| AnnualOtherTaxableIncome      | Place for other incomes                    |
| AnnualOtherUntaxableIncome    | Place for other incomes                    |
| Annual401KContribution        | Amount added per year                      |
| MonthlyExpenses               | Monthly expenses, not including mortagage  |
| AnnualOtherExpenses           | Fixed amount for other needs               |
| DesiredCashOnHand             | Desired cah on hand, rest into investments |
| Assets                        | A list of assets (5)                       |
| Debts                         | A list of debts (6)                        |

### (3) Social Security wages
These are the annual wages as recordes by social security. You can look up your numbers from https://www.ssa.gov/myaccount/

### (4) Social Security income
Computed internally upon request

### (5) Assets
A list of assets. Each has 

| Parameter  | Meaning                                        |
| -----------|---------------                                 |
| Name       | a name for the asset                           |
| AssetType  | Taxable, retirement 401, etc. See file itself  |
| Value      | Value at start of sim                          |
| RateName   | the name of the rate to apply to it for growth |


### (6) Debts

| Parameter       | Meaning                         |
| -------------   |---------------                  |
| Name            | a name for the debt             |
| DebtType        | Loan, mortgage, etc             |
| Principal       | Amount left at sim start        |
| AnnualRate      | yearly APR (not effective APR)  |
| Payments        | desired number of payments      |
| MonthlyPayment  | monthly payment owed            |

END OF FILE