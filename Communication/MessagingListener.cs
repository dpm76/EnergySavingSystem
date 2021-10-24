using System;
using System.Net;
using System.Net.Sockets;
using Communication.Messages;
using NLog;

namespace Communication
{
    /// <summary>
    /// Servidor del gestor de consumo energético
    /// </summary>
    public class MessagingListener
    {
        private readonly TcpListener _listener;
        private bool _listening;
        private readonly IConversationDispatcher _conversationDispatcher;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="port">Puerto de escucha</param>
        /// <param name="conversationDispatcher">Tratador de los mensajes</param>
        public MessagingListener(int port, IConversationDispatcher conversationDispatcher)
        {
            this._listener = new TcpListener(IPAddress.Any, port);
            this._conversationDispatcher = conversationDispatcher;
        }

        /// <summary>
        /// Puerto de escucha
        /// </summary>
        public int ListenPort
        {
            get { return ((IPEndPoint) this._listener.LocalEndpoint).Port; }
        }

        /// <summary>
        /// Inicia la escucha
        /// </summary>
        public void Start()
        {
            if (this._conversationDispatcher != null)
            {
                this._listening = true;
                this._listener.Start();
                this._listener.BeginAcceptTcpClient(Listen, null);
            }else
            {
                Exception exception = new Exception("No se puede iniciar sin un ConversationDispatcher");
                this._logger.FatalException(exception.Message, exception);
                throw exception;
            }
        }

        /// <summary>
        /// Detiene la escucha
        /// </summary>
        public void Stop()
        {
            this._listening = false;
            this._listener.Stop();
        }

        private void Listen(IAsyncResult asyncResult)
        {
            if (this._listening)
            {
                IMessage message = null;
                try
                {
                    using (TcpClient client = this._listener.EndAcceptTcpClient(asyncResult))
                    {
                        using (NetworkStream stream = client.GetStream())
                        {
                            message = MessageSerializer.Instance.FromStream(stream);
                            this._conversationDispatcher.Dispatch(client.Client.RemoteEndPoint as IPEndPoint, message,
                                                                  stream);
                            stream.Close();
                        }
                        client.Close();
                    }
                    
                }catch(Exception ex)
                {
                    this._logger.WarnException(string.Format("No se ha podido tratar un cliente: mensaje = {0}", (message!=null)?message.ToString():"null"), ex);
                }finally
                {
                    this._listener.BeginAcceptTcpClient(Listen, null);
                }
            }
        }
    }
}
