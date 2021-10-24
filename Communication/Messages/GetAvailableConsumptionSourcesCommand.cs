using System;

namespace Communication.Messages
{
    /// <summary>
    /// Pide las fuentes de datos disponibles
    /// </summary>
    [Serializable]
    public sealed class GetAvailableConsumptionSourcesCommand:IMessage
    {
    }
}
