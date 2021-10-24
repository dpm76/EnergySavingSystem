using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Net;
using System.Text;
using System.Timers;
using Communication;
using Communication.Messages;
using NLog;

namespace EnergyConsumption
{
    /// <summary>
    /// Clase abstracta de los lectores de datos.
    /// Implementa la funcionalidad de lectura, 
    /// almacenamiento y comunicación de la lectura, pero
    /// no cómo realizar la lectura en el dispositivo
    /// físico.
    /// </summary>
    public abstract class DataReader
    {
        private readonly Dictionary<IPAddress,IPEndPoint> _listenersCollection = new Dictionary<IPAddress, IPEndPoint>(3);

        private static readonly CultureInfo _dataFormatter = CultureInfo.GetCultureInfo("EN-us");

        private readonly Timer _currentPowerTimer = new Timer
                                            {
                                                AutoReset = true,
                                                Enabled = false
                                            };

        private readonly Timer _pollingQPowerTimer = new Timer
                                                  {
                                                      AutoReset = true,
                                                      Enabled = false
                                                  };

        private readonly DbConnection _dbConnection;

        public Dictionary<string, float> LastDataReaded { get; private set; }
        public ConsumptionData LastQDataReaded { get; private set; }

        public event EventHandler<DataReaderEventArgs> DataRead;
        public event EventHandler<DataReaderEventArgs> QDataRead;
        
        /// <summary>
        /// Nombre de la fuente de datos
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        /// Nombre del puerto serie donde está el enlace Modbus
        /// </summary>
        public string ComPort { get; set; }

        /// <summary>
        /// Identificador del dispositivo en la conextión Modbus
        /// </summary>
        public byte DeviceId { get; set; }

        /// <summary>
        /// Magnitudes que lee el lector
        /// </summary>
        public string[] Magnitudes { get; set; }

        /// <summary>
        /// Gestor de log
        /// </summary>
        public Logger Log { get; set; }

        protected DataReader(int currentPowerReadInterval, int qPowerReadInterval, DbConnection dbConnection)
        {
            this._dbConnection = dbConnection;

            this._currentPowerTimer.Interval = currentPowerReadInterval;
            this._pollingQPowerTimer.Interval = qPowerReadInterval;
            this._currentPowerTimer.Elapsed += OnCurrentPowerTimerElapsed;
            this._pollingQPowerTimer.Elapsed += OnQPowerTimerElapsed;
        }

        virtual protected bool IsTimeToRead()
        {
            return ((DateTime.Now.Minute%15) == 0);
        }

        private void OnQPowerTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (IsTimeToRead())
            {
                this.LastQDataReaded = this.ReadQPowerData();
                this.Trace(string.Format("Leído {0} en el punto {1}", this.LastQDataReaded, this.SourceId));
                this.StoreData(this.LastQDataReaded.Data, this.LastQDataReaded.TimeStamp);
                this.OnDataReaderEvent(this.QDataRead, this.LastQDataReaded.TimeStamp);
                this.NotifyQData(this.LastQDataReaded.Data, this.LastQDataReaded.TimeStamp);
            }
        }

        private void OnCurrentPowerTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.LastDataReaded = this.ReadCurrentPowerData();
            //Trace
            StringBuilder dataStr = new StringBuilder();
            foreach (KeyValuePair<string, float> keyValuePair in LastDataReaded)
            {
                dataStr.Append(string.Format("{0} = {1}; ", keyValuePair.Key, keyValuePair.Value));
            }
            this.Trace(string.Format("Leído '{0}' en el punto {1}", dataStr, this.SourceId));
            //End Trace
            this.OnDataReaderEvent(this.DataRead, DateTime.UtcNow);
            this.NotifyData(this.LastDataReaded);
        }

        /// <summary>
        /// Inicia el lector de datos
        /// </summary>
        public virtual void Start()
        {
            this._currentPowerTimer.Start();
            this._pollingQPowerTimer.Start();
        }

        /// <summary>
        /// Detiene el lector de datos
        /// </summary>
        public virtual void Stop()
        {
            this._currentPowerTimer.Stop();
            this._pollingQPowerTimer.Stop();
        }

        /// <summary>
        /// Registra un subscriptor
        /// </summary>
        /// <param name="endPoint">Punto remoto donde escuchará el subscriptor</param>
        public void RegisterListerner(IPEndPoint endPoint)
        {
            if (!this._listenersCollection.ContainsKey(endPoint.Address))
            {
                this._listenersCollection.Add(endPoint.Address, endPoint);
            }else
            {
                this._listenersCollection[endPoint.Address] = endPoint;
            }
        }

        /// <summary>
        /// Elimina el registro de un subscriptor
        /// </summary>
        /// <param name="address">Dirección del subscriptor</param>
        public void UnregisterListener(IPAddress address)
        {
            if(this._listenersCollection.ContainsKey(address))
            {
                this._listenersCollection.Remove(address);
            }
        }

        private void NotifyData(Dictionary<string, float> dataCollection)
        {
            this.NotifyRemoteListeners(
                new ConsumptionDataMessage
                    {
                        EnergyData = dataCollection,
                        ReadTimeStamp = DateTime.UtcNow,
                        Source = this.SourceId
                    });
        }

        private void NotifyQData(float? data, DateTime timeStamp)
        {
            this.NotifyRemoteListeners(
                new QHourPowerConsumptionRealTimeData
                {
                    Data = data,
                    TimeStamp = timeStamp,
                    SourceId = this.SourceId
                });
        }

        public void NotifyRemoteListeners(IMessage message)
        {
            using (RealTimeSender sender = new RealTimeSender())
            {
                byte[] buffer = MessageSerializer.Instance.ToBuffer(message);
                foreach (IPEndPoint endPoint in _listenersCollection.Values)
                {
                    this.Trace(string.Format("Notificando a {0}, mensaje: {1}", endPoint.Address, message.ToString()));
                    sender.Send(buffer, buffer.Length, endPoint);
                }
            }
        }

        private void StoreData(float? qPower, DateTime timeStamp)
        {
            lock (this._dbConnection)
            {
                this.Trace(string.Format("Almacenando Qdato: timeStamp = {0}, qPower = {1}", timeStamp, qPower));

                //Comprobar si ya hay un valor
                string sqlSentence =
                    string.Format("SELECT COUNT(*) FROM Consumption WHERE Source = '{0}' AND ReadTime = '{1}'",
                                  this.SourceId, timeStamp.ToUniversalTime().ToString(_dataFormatter));
                int count;
                using (DbCommand command = this._dbConnection.CreateCommand())
                {
                    command.CommandText = sqlSentence;
                    count = (int)command.ExecuteScalar();
                }

                if (count == 0)
                {
                    sqlSentence =
                        (qPower != null)
                            ? string.Format(
                                "INSERT INTO Consumption (Source, ReadTime, Energy) VALUES ('{0}','{1}',{2})",
                                this.SourceId, timeStamp.ToUniversalTime().ToString(_dataFormatter),
                                ((float) qPower).ToString("0.##", _dataFormatter))
                            : string.Format("INSERT INTO Consumption (Source, ReadTime) VALUES ('{0}','{1}')",
                                            this.SourceId, timeStamp.ToUniversalTime().ToString(_dataFormatter));
                }else
                {
                    sqlSentence =
                        (qPower != null)
                            ? string.Format(
                                "UPDATE Consumption SET Energy = {0} WHERE Source = '{1}' AND ReadTime = '{2}'",
                                ((float)qPower).ToString("0.##", _dataFormatter),
                                this.SourceId, timeStamp.ToUniversalTime().ToString(_dataFormatter))
                            : string.Format(
                                "UPDATE Consumption SET Energy = NULL WHERE Source = '{0}' AND ReadTime = '{1}'",
                                this.SourceId, timeStamp.ToUniversalTime().ToString(_dataFormatter));
                }

                using (DbCommand command = this._dbConnection.CreateCommand())
                {
                    command.CommandText = sqlSentence;
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Lee los datos actuales del consumo energético
        /// </summary>
        protected abstract Dictionary<string, float> ReadCurrentPowerData();

        /// <summary>
        /// Lee el dato de consumo cuarto-horario
        /// </summary>
        /// <returns></returns>
        protected abstract ConsumptionData ReadQPowerData();

        /// <summary>
        /// Crea una instancia de lector desde su nombre de clase
        /// </summary>
        /// <param name="className">Nombre de clase (assemblyName.className)</param>
        /// <param name="sourceId">Nombre de la fuente</param>
        /// <param name="comPort">Puerto serie donde se realiza la lectura</param>
        /// <param name="deviceId">Identificador del RTU</param>
        /// <param name="currentPowerReadInterval">Intervalo de lectura de datos en tiempo real</param>
        /// <param name="qPowerReadInterval">Intervalo de lectura para datos Q-horarios</param>
        /// <param name="dbConnection">Conexión a la base de datos</param>
        /// <returns></returns>
        public static DataReader Create(string className, string sourceId, string comPort, byte deviceId, int currentPowerReadInterval, int qPowerReadInterval, DbConnection dbConnection)
        {
            DataReader dataReader = null;

            Type dataReaderType = Type.GetType(className, true);
            if(dataReaderType != null)
            {
                dataReader = (DataReader)dataReaderType.GetConstructor(new[] {typeof (int), typeof (int), typeof (DbConnection)})
                    .Invoke(new object[] {currentPowerReadInterval, qPowerReadInterval, dbConnection});

                dataReader.SourceId = sourceId;
                dataReader.DeviceId = deviceId;
                dataReader.ComPort = comPort;
            }

            return dataReader;
        }

        private void OnDataReaderEvent(EventHandler<DataReaderEventArgs> eventHandler, DateTime timeStamp)
        {
            if(eventHandler != null)
            {
                eventHandler(this, new DataReaderEventArgs(timeStamp));
            }
        }

        private void Trace(string message)
        {
            if(this.Log != null)
            {
                this.Log.Trace(message);
            }
        }
    }
}
