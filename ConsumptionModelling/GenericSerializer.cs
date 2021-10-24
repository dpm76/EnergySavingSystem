using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ConsumptionModelling
{
    /// <summary>
    /// Serializador del modelo
    /// </summary>
    public static class GenericSerializer<T>
    {
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(T));

        public static void Serialize(Stream stream, T obj)
        {
            using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
            {
                _serializer.Serialize(writer, obj);
            }
        }

        public static void Serialize(string filePath, T obj)
        {
            Serialize(new FileStream(filePath,FileMode.Create, FileAccess.Write), obj);
        }

        public static T Deserialize(Stream stream)
        {
            using (XmlTextReader reader = new XmlTextReader(stream))
            {
                return (T)_serializer.Deserialize(reader);
            }
        }

        public static T Deserialize(string filePath)
        {
            return Deserialize(new FileStream(filePath, FileMode.Open, FileAccess.Read));
        }
    }
}
