using System;

namespace Communication.Messages
{
    /// <summary>
    /// Mensaje para eliminar las alertas de las fuentes
    /// </summary>
    [Serializable]
    public sealed class DeleteSourceAlerts:IMessage
    {
        /// <summary>
        /// Indica si se eliminarán las alertas de todas las fuentes
        /// </summary>
        public bool All { get; set; }
        /// <summary>
        /// Nombre de la fuente. Si All es true, se ignorará.
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
