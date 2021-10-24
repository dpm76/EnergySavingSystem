
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ConsumptionDaemon
{
    [Serializable]
    public sealed class DataReadersConfig
    {
        private const string DEFAULT_FILE_NAME = @"DataReaders.config.xml";
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(DataReadersConfig));
        private static DataReadersConfig _instance;

        public static DataReadersConfig Instance
        {
            get
            {
                if(_instance == null)
                {
                    Load();
                }

                return _instance;
            }
        }

        public List<DataReaderConfigItem> DataReadersList { get; set; }

        public static bool Load()
        {
            return Load(Directory.GetCurrentDirectory() + "\\" + DEFAULT_FILE_NAME);
        }

        public static bool Load(string filePath)
        {
            _instance = null;

            if (File.Exists(filePath))
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    using (XmlTextReader textReader = new XmlTextReader(stream))
                    {
                        _instance = (DataReadersConfig) _serializer.Deserialize(textReader);
                        textReader.Close();
                    }
                    stream.Close();
                }
            }

            return _instance != null;
        }

        public void Save()
        {
            this.Save(Directory.GetCurrentDirectory() + "\\" + DEFAULT_FILE_NAME);
        }

        public void Save(string filePath)
        {
            using(FileStream stream = File.Open(filePath, FileMode.Create, FileAccess.Write))
            {
                using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
                {
                    _serializer.Serialize(writer, this);
                    writer.Close();
                }
                stream.Close();
            }
        }

        public static void CreateFakeConfig()
        {
            _instance = new DataReadersConfig
                            {
                                DataReadersList = new List<DataReaderConfigItem>
                                                      {
                                                          new DataReaderConfigItem
                                                              {
                                                                  ClassName = "EnergyConsumption.FakeDataReader",
                                                                  ComPort = "COM1",
                                                                  DeviceId = 1,
                                                                  SourceId = "FakeSource"
                                                              }
                                                      }
                            };
        }
    }
}
