using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.PersonalFinance.Model
{
    public enum ColType
    {   Info,
        Income,
        Expense,
        Asset,
        Debt,
    }
    /// <summary>
    /// Column name
    /// </summary>
    public class Col : IEquatable<Col>
    {
        public ColType Type { get; private set; }
        public string Field { get; private set; }
        public string Name { get; private set; }

        public override string ToString()
        {
            return $"{Type}: {Name} {Field}";
        }

        public static Col Make(ColType type, string field, string name = "")
        {
            return new Col { Type = type, Field = field, Name = name};
        }

        #region IEquatable
        public bool Equals(Col other)
        {
            // Check whether the compared object is null.
            if (Object.ReferenceEquals(other, null)) return false;

            // Check whether the compared object references the same data.
            if (Object.ReferenceEquals(this, other)) return true;

            // Check whether the objects’ properties are equal.
            return Type == other.Type && Name == other.Name && Field == other.Field;
        }

        // if Equals returns true, GetHashCode must return same value
        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ Name.GetHashCode() ^ Field.GetHashCode();
        }

        #endregion
    }
}
