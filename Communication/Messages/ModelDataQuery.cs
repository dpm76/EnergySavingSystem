using System;

namespace Communication.Messages
{
    /// <summary>
    /// Obtiene datos del modelo de consumo
    /// </summary>
    [Serializable]
    public class ModelDataQuery:IMessage
    {
        /// <summary>
        /// Indica si se quieren datos del modelo de ahorro (true) o
        /// del modelo actual (false)
        /// </summary>
        public bool SaveModel { get; set; }
        /// <summary>
        /// Fecha/hora inicial
        /// </summary>
        public DateTime Since { get; set; }
        /// <summary>
        /// Fecha/hora final
        /// </summary>
        public DateTime Until { get; set; }
        /// <summary>
        /// Fuente de datos
        /// </summary>
        public string SourceId { get; set; }
    }
}
