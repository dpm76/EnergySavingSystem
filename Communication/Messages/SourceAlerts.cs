using System;
using System.Collections.Generic;

namespace Communication.Messages
{
    /// <summary>
    /// Mensaje con las alertas de una fuente
    /// </summary>
    [Serializable]
    public sealed class SourceAlerts:IMessage
    {
        /// <summary>
        /// Indica que no hay alertas o que se han enviado todas.
        /// Si es true el resto de propiedades serán ignoradas
        /// </summary>
        public bool Empty { get; set; }
        public string SourceId { get; set; }
        public List<DateTime> AlertTimeStamps { get; set; }

        #region Miembros de IMessage

        string IMessage.ToString()
        {
            return ToString() + "; SourceId = " + SourceId + "; AlertTimeStamps = " + AlertTimeStamps;
        }

        #endregion
    }
}
