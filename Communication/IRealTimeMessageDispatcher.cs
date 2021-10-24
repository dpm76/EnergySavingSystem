using Communication.Messages;

namespace Communication
{
    /// <summary>
    /// Interfaz para los tratadores de los mensajes
    /// </summary>
    public interface IRealTimeMessageDispatcher
    {
        /// <summary>
        /// Procesa un mensaje
        /// </summary>
        /// <param name="message">Mensaje</param>
        void Dispatch(IMessage message);
    }
}