using System;

namespace EnergyConsumption
{
    /// <summary>
    /// Argumentos de los eventos del lector de datos
    /// </summary>
    public class DataReaderEventArgs:EventArgs
    {
        private readonly DateTime _timeStamp;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="timeStamp">Marca de hora del evento (UTC)</param>
        internal DataReaderEventArgs(DateTime timeStamp)
        {
            this._timeStamp = timeStamp.ToUniversalTime();
        }

        /// <summary>
        /// Marca de hora del evento (UTC)
        /// </summary>
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
        }
    }
}
