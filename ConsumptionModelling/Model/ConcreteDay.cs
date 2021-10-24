using System;

namespace ConsumptionModelling.Model
{
    /// <summary>
    /// Dia concreto del año. 
    /// </summary>
    [Serializable]
    public sealed class ConcreteDay:DayLevel
    {
        public short Year { get; set; }
        public short Month { get; set; }
        public short Day { get; set; }

        public override bool Equals(object obj)
        {
            ConcreteDay other = obj as ConcreteDay;
            return this.Equals(other);
        }

        public bool Equals(ConcreteDay other)
        {
            return
                (other != null) &&
                (other.Year == Year) && 
                (other.Month == Month) && 
                (other.Day == Day);
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
                int result = Year.GetHashCode();
                result = (result*397) ^ Month.GetHashCode();
                result = (result*397) ^ Day.GetHashCode();
                return result;
            }
        }

        public override string ToString()
        {
            return
                this.Day + "; " +
                this.Month + "; " +
                this.Year + "; " +
                base.ToString() + "; ";
        }
    }
}
