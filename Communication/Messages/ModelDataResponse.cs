
using System;
using System.Collections.Generic;

namespace Communication.Messages
{
    /// <summary>
    /// Respuesta con datos del modelo de consumo
    /// </summary>
    [Serializable]
    public class ModelDataResponse:IMessage
    {
        /// <summary>
        /// Datos
        /// </summary>
        public List<float> Data { get; set; }
    }
}
