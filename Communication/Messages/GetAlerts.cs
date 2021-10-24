using System;

namespace Communication.Messages
{
    /// <summary>
    /// Mensaje para pedir las alertas
    /// </summary>
    [Serializable]
    public sealed class GetAlerts:IMessage
    {
        /// <summary>
        /// Indica si se piden todas o las de una fuente concreta
        /// </summary>
        public bool All { get; set; }

        /// <summary>
        /// Nombre de la fuente de las alertas. Si All es true, se ignora.
        /// </summary>
        public string SourceId { get; set; }

        #region Miembros de IMessage

        string IMessage.ToString()
        {
            return ToString() + "; All = " + All + "; SourceId = " + SourceId;
        }

        #endregion
    }
}
