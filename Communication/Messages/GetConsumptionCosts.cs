using System;

namespace Communication.Messages
{
    /// <summary>
    /// Mensaje para obtener los costes del cosumo para un intervalo
    /// </summary>
    [Serializable]
    public sealed class GetConsumptionCosts:IMessage
    {
        /// <summary>
        /// Identificador de la fuente de datos
        /// </summary>
        public string SourceId { get; set; }
        /// <summary>
        /// Inicio del intervalo
        /// </summary>
        public DateTime Since { get; set; }
        /// <summary>
        /// Final del intervalo
        /// </summary>
        public DateTime Until { get; set; }

        public override string ToString()
        {
            return base.ToString() + string.Format(": SourceId = {0}; Since = {1}; Until = {2}; ", SourceId, Since, Until);
        }
    }
}
