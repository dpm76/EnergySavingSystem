using System;
using System.Collections.Generic;

namespace Communication.Messages
{
    /// <summary>
    /// Respuesta de datos históricos de consumo Q-horario
    /// </summary>
    [Serializable]
    public class QHourConsumptionHistoricDataResponse:IMessage
    {
        public List<float?> Data { get; set; }
    }
}
