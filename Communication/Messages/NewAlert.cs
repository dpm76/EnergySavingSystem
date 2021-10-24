using System;

namespace Communication.Messages
{
    /// <summary>
    /// Mensaje de nueva alerta
    /// </summary>
    [Serializable]
    public sealed class NewAlert:IMessage
    {
        /// <summary>
        /// Nombre de la fuente
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
