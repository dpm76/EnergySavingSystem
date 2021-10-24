using System.Net;
using System.Net.Sockets;
using Communication.Messages;

namespace Communication
{
    /// <summary>
    /// Tratador de mensajes con conversaciones
    /// </summary>
    public interface IConversationDispatcher
    {
        /// <summary>
        /// Trata el mensaje
        /// </summary>
        /// <param name="remoteEndPoint">Enlace remoto</param>
        /// <param name="message">Mensaje recibido</param>
        /// <param name="stream">Flujo con la entidad remota</param>
        void Dispatch(IPEndPoint remoteEndPoint, IMessage message, NetworkStream stream);
    }
}