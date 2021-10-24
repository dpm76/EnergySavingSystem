using System;
using System.Net;
using System.Net.Sockets;
using Communication.Messages;
using NLog;

namespace Communication
{
    /// <summary>
    /// Servidor Udp
    /// </summary>
    public class RealTimeListener
    {
        private readonly UdpClient _listener;

        private bool _listening;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="port">Puerto de escucha</param>
        public RealTimeListener(int port)
        {
            this._listener = new UdpClient(port);
        }

        /// <summary>
        /// Tratador de mensajes
        /// </summary>
        public IRealTimeMessageDispatcher MessageDispatcher { get; set; }

        /// <summary>
        /// Inicia la escucha
        /// </summary>
        public void Start()
        {
            this._listening = true;
            this._listener.BeginReceive(DataReceived, null);
        }

        /// <summary>
        /// Detiene la escucha
        /// </summary>
        public void Stop()
        {
            this._listening = false;
            lock (this._listener)
            {
                this._listener.Close();
            }
        }

        private void DataReceived(IAsyncResult result)
        {
            if (this._listening)
            {
                lock (_listener)
                {
                    try
                    {
                        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                        byte[] buffer = this._listener.EndReceive(result, ref endPoint);

                        if (this.MessageDispatcher != null)
                        {
                            this.MessageDispatcher.Dispatch(MessageSerializer.Instance.FromBuffer(buffer));
                        }
                    }
                    catch (Exception ex)
                    {
                        this._logger.WarnException("No se ha podido tratar una entrada de datos" + ex, ex);
                    }
                    finally
                    {
                        this._listener.BeginReceive(DataReceived, null);
                    }
                }
            }
        }
    }
}
