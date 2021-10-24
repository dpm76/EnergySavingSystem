using System;

namespace ConsumptionModelling.Model
{
    /// <summary>
    /// Nivel de día de semana
    /// </summary>
    [Serializable]
    public sealed class WeekDayLevel:DayLevel
    {
        public DayOfWeek DayOfWeek { get; set; }

        public override bool Equals(object obj)
        {
            WeekDayLevel other = obj as WeekDayLevel;
            return this.Equals(other);
        }

        public bool Equals(WeekDayLevel other)
        {
            return base.Equals(other) && (other.DayOfWeek == this.DayOfWeek);
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
                return (base.GetHashCode()*397) ^ DayOfWeek.GetHashCode();
            }
        }

        public override string ToString()
        {
            return
                this.DayOfWeek + "; " +
                base.ToString() + "; ";
        }
    }
}
