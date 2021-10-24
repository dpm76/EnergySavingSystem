using System;

namespace EnergyConsumption
{
    public class ConsumptionData
    {
        public float? Data { get; set; }
        public DateTime TimeStamp { get; set; }

        public override string ToString()
        {
            return string.Format("Data = {0}; TimeStamp = {1}; ", Data.HasValue?Data.Value.ToString():"null", TimeStamp);
        }
    }
}
