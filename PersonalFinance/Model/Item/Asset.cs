using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Lomont.PersonalFinance.Model.Item
{
    [Serializable]
    public class Asset
    {
        public Asset(string name, AssetType type)
        {
            Name = name;
            AssetType = type;

        }
        public string Name;
        [JsonConverter(typeof(StringEnumConverter))]
        public AssetType AssetType;

        public double Value;

        /// <summary>
        /// Name of the rate to apply
        /// </summary>
        public string RateName;

        // compute growth, return increase
        public double Grow(Rate rate, int year)
        {
            var rate1 = rate.GetValue(year);
            var increase = ((int) (Value * rate1 * 100))/100.0; // round to dollar
            Value += increase;
            return increase;
        }
    }
}
