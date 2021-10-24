using System;

namespace Communication.Messages
{
    /// <summary>
    /// Comando para eleiminar el registro de un elemento que 
    /// escucha los datos de consumo
    /// </summary>
    [Serializable]
    public sealed class UnregisterConsumptionDataReaderListenerCommand : IMessage
    {
        /// <summary>
        /// Origen de datos donde se quiere suscribir
        /// </summary>
        public string SourceId { get; set; }

        public override string ToString()
        {
            return base.ToString() + string.Format("({0})", SourceId);
        }
    }
}
