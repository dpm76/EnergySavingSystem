using System;

namespace Communication.Messages
{
    /// <summary>
    /// Comando para registrar un elemento que escucha los datos de consumo
    /// </summary>
    [Serializable]
    public sealed class RegisterConsumptionDataReaderListenerCommand:IMessage
    {
        /// <summary>
        /// Puerto donde escucha
        /// </summary>
        public int ListenPort { get; set; }
        /// <summary>
        /// Origen de datos donde se quiere suscribir
        /// </summary>
        public string SourceId { get; set; }

        public override string ToString()
        {
            return base.ToString() + string.Format("({0},{1})", ListenPort, SourceId);
        }
    }
}
