using System;
using System.Collections.Generic;

namespace DynamicReports.RealTimeConsumption
{
    /// <summary>
    /// Estrucuctura de datos para los datos en tiempo real
    /// </summary>
    public class RealTimeData
    {
        private DateTime _timeStamp;

        /// <summary>
        /// Marca de tiempo (se utilizará UTC)
        /// </summary>
        public DateTime TimeStamp
        {
            get { return this._timeStamp; }
            set { this._timeStamp = value.ToUniversalTime(); }
        }
        /// <summary>
        /// Colección de datos.
        /// La clave del diccionario es el identificador de la magnitud de datos
        /// </summary>
        public Dictionary<string, float> DataCollection { get; set; }

        /// <summary>
        /// Calcula el máximo valor de todos los datos
        /// </summary>
        /// <returns></returns>
        public float MaxValue()
        {
            float maxValue = 0;
            foreach (float value in DataCollection.Values)
            {
                if(value > maxValue)
                {
                    maxValue = value;
                }
            }

            return maxValue;
        }
    }
}
