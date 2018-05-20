using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Lomont.PersonalFinance.Model.Item
{
    [Serializable]
    public class Debt
    {
        public Debt(string name, AssetType type)
        {
            Name = name;
            DebtType = type;

        }
        public string Name;
        [JsonConverter(typeof(StringEnumConverter))]
        public AssetType DebtType;
        public double Principal;
        public double AnnualRate;
        public double Payments;
        public double MonthlyPayment;
    }
}
