using System;
using System.Collections.Generic;
using System.Text;

namespace ConsumptionModelling
{
    /// <summary>
    /// Coste de la energía en un periodo
    /// </summary>
    [Serializable]
    public struct PowerCost
    {
        public DateTime Since { get; set; }
        public DateTime Until { get; set; }
        public CostPeriod[] WinterCostPeriods { get; set; }
        public CostPeriod[] SummerCostPeriods { get; set; }

        /// <summary>
        /// Instancia por defecto
        /// </summary>
        public static PowerCost Dummy = new PowerCost
        {
            Since = DateTime.MinValue,
            Until = DateTime.MaxValue,
            WinterCostPeriods =
                new[] { new CostPeriod { Since = 0, Until = 24, Price = 0f } },
            SummerCostPeriods =
                new[] { new CostPeriod { Since = 0, Until = 24, Price = 0f } }
        };


        public override string ToString()
        {
            return string.Format("Since = {0}; Until = {1}; WinterCostPeriods = {2}; SummerCostPeriods = {3}; ", Since, Until, BuildCostPeriodsString(WinterCostPeriods), BuildCostPeriodsString(SummerCostPeriods));
        }

        private static string BuildCostPeriodsString(IEnumerable<CostPeriod> costPeriods)
        {
            StringBuilder costPeriodsStr = new StringBuilder();
            foreach (CostPeriod costPeriod in costPeriods)
            {
                costPeriodsStr.Append(costPeriod + "; ");
            }

            return costPeriodsStr.ToString();
        }

        public override bool Equals(object obj)
        {
            PowerCost other = (PowerCost) obj;

            return
                CostPeriodsCollectionAreEqual(this.WinterCostPeriods, other.WinterCostPeriods) &&
                CostPeriodsCollectionAreEqual(this.SummerCostPeriods, other.SummerCostPeriods) && 
                other.Since.Equals(Since) &&
                other.Until.Equals(Until);
        }

        private static bool CostPeriodsCollectionAreEqual(CostPeriod[] periodCosts1, CostPeriod[] periodCosts2)
        {
            bool costPeriodsAreEqual = (periodCosts1.Length == periodCosts2.Length);

            for (int i = 0; costPeriodsAreEqual && (i < periodCosts1.Length); ++i)
            {
                costPeriodsAreEqual &= periodCosts1[i].Equals(periodCosts2[i]);
            }

            return costPeriodsAreEqual;
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
                result = (result*397) ^ (WinterCostPeriods != null ? WinterCostPeriods.GetHashCode() : 0);
                return result;
            }
        }

    }

}
