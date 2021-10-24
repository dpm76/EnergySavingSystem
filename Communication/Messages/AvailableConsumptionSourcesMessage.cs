using System;
using System.Collections.Generic;

namespace Communication.Messages
{
    /// <summary>
    /// Indica las fuentes disponibles
    /// </summary>
    [Serializable]
    public sealed class AvailableConsumptionSourcesMessage:IMessage
    {
        public Dictionary<string, string[]> MagnitudesBySources { get; set; }

        public override string ToString()
        {
            return base.ToString() + ": " + this.MagnitudesBySources;
        }
    }
}
