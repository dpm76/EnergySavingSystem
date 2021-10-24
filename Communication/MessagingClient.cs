using System;
using System.Net.Sockets;
using Communication.Messages;
using NLog;

namespace Communication
{
    /// <summary>
    /// Cliente para el envío de mensajes
    /// </summary>
    public class MessagingClient:IDisposable
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Delegado de la conversación con el enlace
        /// </summary>
        /// <param name="message">Mensaje recibido</param>
        /// <param name="stream">Flujo de intercambio con el enlace</param>
        /// <returns></returns>
        public delegate object ConversationDelegate(IMessage message, NetworkStream stream);

        private readonly TcpClient _client;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hostName">Nombre del enlace remoto</param>
        /// <param name="port">Puerto de escucha del enlace remoto</param>
        public MessagingClient(string hostName, int port)
        {
            try
            {
                this._client = new TcpClient(hostName, port);
            }
            catch (Exception ex)
            {
                this._logger.ErrorException(string.Format("No se ha podido conectar con el host remoto {0}:{1}", hostName, port), ex);
            }
            
        }

        /// <summary>
        /// Envía un mensaje sin iniciar una conversación
        /// </summary>
        /// <param name="message">Mensaje</param>
        public NetworkStream Send(IMessage message)
        {
            NetworkStream stream = null;
            if (this._client != null)
            {
                try
                {
                    stream = this._client.GetStream();
                    MessageSerializer.Instance.ToStream(stream, message);
                }
                catch (Exception ex)
                {
                    this._logger.ErrorException(string.Format("No se ha podido enviar el mensaje {0}", message), ex);
                }
            }
            return stream;
        }

        /// <summary>
        /// Envía un mensaje iniciando una conversación
        /// </summary>
        /// <param name="message">Mensaje</param>
        /// <param name="conversationDelegate">Delegado de la conversación con el enlace remoto</param>
        public object Send(IMessage message, ConversationDelegate conversationDelegate)
        {
            object res = null;
            
            NetworkStream stream = this.Send(message);
            if (stream != null)
            {
                res = conversationDelegate(MessageSerializer.Instance.FromStream(stream), stream);
            }
            return res;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (this._client != null)
            {
                this._client.Close();
            }
        }
    }
}
