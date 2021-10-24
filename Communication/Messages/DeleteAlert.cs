using System;

namespace Communication.Messages
{
    /// <summary>
    /// Mensaje para eliminar una alerta
    /// </summary>
    [Serializable]
    public sealed class DeleteAlert:IMessage
    {
        /// <summary>
        /// Fuente de la alerta
        /// </summary>
        public string SourceId { get; set; }
        /// <summary>
        /// Marca horaria de la alerta
        /// </summary>
        public DateTime TimeStamp { get; set; }

        #region Miembros de IMessage

        string IMessage.ToString()
        {
            return ToString() + "; SourceId = " + SourceId + "; TimeStamp = " + TimeStamp;
        }

        #endregion
    }
}
