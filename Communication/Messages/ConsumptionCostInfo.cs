using System;

namespace Communication.Messages
{
    /// <summary>
    /// Información sobre el coste del consumo para un intervalo de tiempo
    /// </summary>
    [Serializable]
    public sealed class ConsumptionCostInfo : IMessage
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
        /// <summary>
        /// Coste energético ya realizado
        /// </summary>
        public float CommitedPowerCost { get; set; }
        /// <summary>
        /// Coste estimado para el intervalo de tiempo completo
        /// </summary>
        public float EstimatedPowerCost { get; set; }
        /// <summary>
        /// Ahorro estimado para el intervalo de tiempo completo
        /// </summary>
        public float EstimatedPowerCostSaved { get; set; }

        public override string ToString()
        {
            return base.ToString() + string.Format("; SourceId = {0}; Since = {1}; Until = {2}; CommitedPowerCost = {3}; EstimatedPowerCost = {4}; EstimatedPowerCostSaved = {5};", SourceId, Since, Until, CommitedPowerCost, EstimatedPowerCost, EstimatedPowerCostSaved);
        }
    }
}
