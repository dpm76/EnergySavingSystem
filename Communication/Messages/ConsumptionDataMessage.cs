using System;
using System.Collections.Generic;
using System.Text;

namespace Communication.Messages
{
    /// <summary>
    /// Mensaje de datos de consumo energético
    /// </summary>
    [Serializable]
    public sealed class ConsumptionDataMessage: IMessage
    {
        /// <summary>
        /// Fuente de datos
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// Marca horaria de la lectura (UTC)
        /// </summary>
        public DateTime ReadTimeStamp { get; set; }
        /// <summary>
        /// Datos de energía por magnitud
        /// </summary>
        public Dictionary<string,float> EnergyData { get; set; }

        public override string ToString()
        {
            StringBuilder energyDataStr = new StringBuilder();
            foreach (KeyValuePair<string, float> keyValuePair in EnergyData)
            {
                energyDataStr.Append(string.Format("{0} = {1}; ", keyValuePair.Key, keyValuePair.Value));
            }
            return base.ToString() + string.Format("({0},{1},{2})", this.Source, this.ReadTimeStamp, energyDataStr);
        }
    }
}
