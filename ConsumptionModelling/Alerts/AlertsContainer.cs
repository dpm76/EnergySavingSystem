using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ConsumptionModelling.Alerts
{
    /// <summary>
    /// Contiene las alertas que se han producido
    /// </summary>
    [Serializable]
    public sealed class AlertsContainer
    {
        private static AlertsContainer _instance;
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(List<DateTime>));
        private const string DEFAULT_DIRECTORY_NAME = @"alerts";
        private const string DEFAULT_FILE_EXTENSION = @"xml";
        private Dictionary<string,List<DateTime>> _alerts = new Dictionary<string, List<DateTime>>();

        /// <summary>
        /// Indica si guarda automáticamente los cambios
        /// </summary>
        public static bool AutoSave { get; set; }
        
        public static AlertsContainer Instance
        {
            get
            {
                if(_instance == null)
                {
                    if(!Load())
                    {
                        _instance =  new AlertsContainer();
                    }
                }

                return _instance;
            }
        }

        public Dictionary<string, List<DateTime>> Alerts
        {
            get { return _alerts; }
            set { _alerts = value; }
        }

        public static bool Load()
        {
            return Load(Directory.GetCurrentDirectory() + "\\" + DEFAULT_DIRECTORY_NAME, DEFAULT_FILE_EXTENSION);
        }

        public static bool Load(string directoryName, string fileExtension)
        {
            bool loaded = false;

            if(Directory.Exists(directoryName) && (Directory.GetFiles(directoryName,"*." + fileExtension).Length > 0))
            {
                _instance = new AlertsContainer();

                lock (_instance._alerts)
                {
                    foreach (string fileName in Directory.GetFiles(directoryName, "*." + fileExtension))
                    {
                        FileInfo fi = new FileInfo(fileName);

                        using (FileStream stream = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read))
                        {
                            using (XmlTextReader reader = new XmlTextReader(stream))
                            {
                                _instance.Alerts.Add(fi.Name.Split('.')[0], (List<DateTime>) _serializer.Deserialize(reader));
                                reader.Close();
                            }
                            stream.Close();
                        }
                    }
                }

                loaded = true;
            }

            return loaded;
        }

        public void Save()
        {
            this.Save(Directory.GetCurrentDirectory() + "\\" + DEFAULT_DIRECTORY_NAME, DEFAULT_FILE_EXTENSION);
        }

        public void Save(string sourceId)
        {
            this.Save(sourceId, Directory.GetCurrentDirectory() + "\\" + DEFAULT_DIRECTORY_NAME, DEFAULT_FILE_EXTENSION);
        }

        public void Save(string fullDirectoryName, string fileExtension)
        {
            foreach (string sourceId in _alerts.Keys)
            {
                this.Save(sourceId, fullDirectoryName, fileExtension);
            }
        }

        public void Save(string sourceId, string fullDirectoryName, string fileExtension)
        {
            lock (this._alerts)
            {
                if (this._alerts.ContainsKey(sourceId))
                {
                    if (!Directory.Exists(fullDirectoryName))
                    {
                        Directory.CreateDirectory(fullDirectoryName);
                    }
                    using (
                        FileStream stream = new FileStream(fullDirectoryName + "\\" + sourceId + "." + fileExtension,
                                                           FileMode.Create, FileAccess.Write))
                    {
                        using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
                        {
                            _serializer.Serialize(writer, this._alerts[sourceId]);
                            writer.Close();
                        }
                        stream.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Añade una alerta
        /// </summary>
        /// <param name="sourceId">Nombre de la fuente</param>
        /// <param name="timeStamp">Marca de hora de la alerta (UTC)</param>
        public void AddAlert(string sourceId, DateTime timeStamp)
        {
            lock (this._alerts)
            {
                if (!this._alerts.ContainsKey(sourceId))
                {
                    this._alerts.Add(sourceId, new List<DateTime>());
                }
                this._alerts[sourceId].Add(timeStamp.ToUniversalTime());
            }
            if(AutoSave)
            {
                this.Save(sourceId);
            }
        }

        /// <summary>
        /// Elimina una alerta
        /// </summary>
        /// <param name="sourceId">Nombre de la fuente</param>
        /// <param name="timeStamp">Marca horaria de la alerta (UTC)</param>
        public void RemoveAlert(string sourceId, DateTime timeStamp)
        {
            if(this._alerts.ContainsKey(sourceId))
            {
                lock (this._alerts)
                {
                    this._alerts[sourceId].Remove(timeStamp.ToUniversalTime());
                }
                if(AutoSave)
                {
                    this.Save(sourceId);
                }
            }
        }

        /// <summary>
        /// Elimina todas las alertas de una fuente
        /// </summary>
        /// <param name="sourceId">Nombre de la fuente</param>
        public void ClearSource(string sourceId)
        {
            if(this._alerts.ContainsKey(sourceId))
            {
                lock (this._alerts)
                {
                    this._alerts[sourceId].Clear();
                }
                if(AutoSave)
                {
                    File.Delete(Directory.GetCurrentDirectory() + "\\" + DEFAULT_DIRECTORY_NAME + "\\" + sourceId + "." + DEFAULT_FILE_EXTENSION);
                }
            }
        }

        /// <summary>
        /// Elimina todas las alertas de las listas
        /// </summary>
        public void ClearAll()
        {
            lock (this._alerts)
            {
                foreach (List<DateTime> alertList in _alerts.Values)
                {
                    alertList.Clear();
                }
            }
            if(AutoSave)
            {
                Directory.Delete(Directory.GetCurrentDirectory() + "\\" + DEFAULT_DIRECTORY_NAME, true);
            }
        }
    }
}
