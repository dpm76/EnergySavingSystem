using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Communication.Messages
{
    /// <summary>
    /// Serializador de los mensajes
    /// </summary>
    public class MessageSerializer
    {
        private static MessageSerializer _instance;

        private readonly BinaryFormatter _innerSerializer = new BinaryFormatter();

        /// <summary>
        /// Instancia única
        /// </summary>
        public static MessageSerializer Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new MessageSerializer();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Crea el mensaje desde un array de bytes
        /// </summary>
        /// <param name="buffer">Buffer desde donde se leerán los datos</param>
        /// <returns></returns>
        public IMessage FromBuffer(byte[] buffer)
        {
            IMessage message;
            using(MemoryStream ms = new MemoryStream(buffer))
            {
                message = this.FromStream(ms);
            }

            return message;
        }

        /// <summary>
        /// Crea el mensaje desde un stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns></returns>
        public IMessage FromStream(Stream stream)
        {
            return (IMessage)this._innerSerializer.Deserialize(stream);
        }

        /// <summary>
        /// Crea un array de bytes que representa el mensaje
        /// </summary>
        /// <param name="message">Mensaje</param>
        /// <returns></returns>
        public byte[] ToBuffer(IMessage message)
        {
            byte[] buffer;
            using (MemoryStream ms = new MemoryStream())
            {
                this._innerSerializer.Serialize(ms, message);
                buffer = ms.ToArray();
            }

            return buffer;
        }

        /// <summary>
        /// Escribe la representación del mensaje en el flujo de entrada
        /// </summary>
        /// <param name="stream">Flujo</param>
        /// <param name="message">Mensaje</param>
        public void ToStream(Stream stream, IMessage message)
        {
            byte[] buffer = this.ToBuffer(message);
            stream.Write(buffer, 0, buffer.Length);
        }
    }
}
