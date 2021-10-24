using System;

namespace ConsumptionModelling
{
    [Serializable]
    public struct CostPeriod
    {
        public short Since { get; set; }
        public short Until { get; set; }
        public float Price { get; set; }

        public override string ToString()
        {
            return string.Format("Since = {0}; Until = {1}; Price = {2}; ", Since, Until, Price);
        }

        public override bool Equals(object obj)
        {
            CostPeriod other = (CostPeriod) obj;
            return other.Since == Since && other.Until == Until && other.Price.Equals(Price);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = Since.GetHashCode();
                result = (result*397) ^ Until.GetHashCode();
                result = (result*397) ^ Price.GetHashCode();
                return result;
            }
        }
    }
}
