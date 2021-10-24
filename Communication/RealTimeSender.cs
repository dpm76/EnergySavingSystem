using System;
using System.Net;
using System.Net.Sockets;
using Communication.Messages;
using NLog;

namespace Communication
{
    /// <summary>
    /// Emisor de datos en tiempo real
    /// </summary>
    public class RealTimeSender:UdpClient
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Envía un mensaje a un destinatario
        /// Indica si se ha enviado (no quiere decir que se haya recibido)
        /// </summary>
        /// <param name="message">Mensaje</param>
        /// <param name="endPoint">Destinatario</param>
        /// <returns></returns>
        public bool Send(IMessage message, IPEndPoint endPoint)
        {
            byte[] buffer = MessageSerializer.Instance.ToBuffer(message);
            return (this.Send(buffer, buffer.Length, endPoint) > 0);
        }

        new public int Send(byte[] dgram, int bytes, IPEndPoint endPoint)
        {
            int bytesSend = -1;
            try
            {
                bytesSend = base.Send(dgram, bytes, endPoint);
            }catch(Exception ex)
            {
                this._logger.WarnException("No se ha podido enviar", ex);
            }

            return bytesSend;
        }

        new public int Send(byte[] dgram, int bytes)
        {
            int bytesSend = -1;
            try
            {
                bytesSend = base.Send(dgram, bytes);
            }catch(Exception ex)
            {
                this._logger.WarnException("No se ha podido enviar", ex);
            }

            return bytesSend;
        }

        new public int Send(byte[] dgram, int bytes, string hostName, int port)
        {
            int bytesSend = -1;
            try
            {
                bytesSend = base.Send(dgram, bytes, hostName, port);
            }
            catch (Exception ex)
            {
                this._logger.WarnException("No se ha podido enviar", ex);
            }

            return bytesSend;
        }
    }
}
