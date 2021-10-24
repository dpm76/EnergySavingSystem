using System;

namespace Communication.Messages
{
    /// <summary>
    /// Consumo Q-Horario en tiempo real
    /// </summary>
    [Serializable]
    public class QHourPowerConsumptionRealTimeData:IMessage
    {
        /// <summary>
        /// Identificador de la fuente
        /// </summary>
        public string SourceId { get; set; }
        /// <summary>
        /// Dato leído
        /// </summary>
        public float? Data { get; set; }
        /// <summary>
        /// Marca horaria
        /// </summary>
        public DateTime TimeStamp { get; set; }

        public override string ToString()
        {
            return base.ToString() + string.Format("({0},{1},{2})", this.SourceId, this.TimeStamp, this.Data);
        }
    }
}
