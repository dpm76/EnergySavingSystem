using System;

namespace EnergySavingManager.Alerts
{
    /// <summary>
    /// Argumentos de los eventos de alerta
    /// </summary>
    public class AlertEventArgs:EventArgs
    {
        /// <summary>
        /// Identificador de la fuente
        /// </summary>
        public string SourceId { get; private set; }

        /// <summary>
        /// Marca horaria
        /// </summary>
        public DateTime TimeStamp { get; private set; }

        internal AlertEventArgs(string sourceId, DateTime timeStamp)
        {
            this.SourceId = sourceId;
            this.TimeStamp = timeStamp;
        }
    }
}
