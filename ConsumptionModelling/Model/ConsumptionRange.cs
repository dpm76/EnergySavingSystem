using System;

namespace ConsumptionModelling.Model
{
    /// <summary>
    /// Consumo para un espacio de tiempo
    /// </summary>
    [Serializable]
    public class ConsumptionRange:ICloneable
    {
        public short Since { get; set; }
        public short Until { get; set; }
        public float Value { get; set; }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            return new ConsumptionRange {Since = Since, Until = Until, Value = Value};
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ConsumptionRange);
        }

        public bool Equals(ConsumptionRange other)
        {
            return 
                (other != null) &&
                (other.Since == this.Since) &&
                (other.Until == this.Until) &&
                (other.Value == this.Value);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = Since.GetHashCode();
                result = (result*397) ^ Until.GetHashCode();
                result = (result*397) ^ Value.GetHashCode();
                return result;
            }
        }

        public static bool operator ==(ConsumptionRange left, ConsumptionRange right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ConsumptionRange left, ConsumptionRange right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return "Since = " + Since + "; Until = " + Until + "; Value = " + Value;
        }
    }
}
