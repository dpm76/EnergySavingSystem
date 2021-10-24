using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.IO;
using System.Net;
using Communication;
using Communication.Messages;
using ConsumptionDaemon.Properties;
using ConsumptionModelling;
using ConsumptionModelling.Alerts;
using EnergyConsumption;
using Licensing;
using Licensing.Activation;
using Licensing.PlatformDependent;
using NLog;

namespace ConsumptionDaemon
{
    /// <summary>
    /// Gestor principal del consumo energético
    /// </summary>
    public class ConsumptionManager
    {
        private static readonly ConsumptionManager _instance = new ConsumptionManager();

        private readonly Dictionary<string, DataReader> _dataReaderCollection = new Dictionary<string, DataReader>();
        private readonly MessagingListener _listener = new MessagingListener(Settings.Default.ListenPort, new ConversationDispatcher());

        private readonly CommitedConsumptionDataProvider _commitedConsumptionDataProvider;
        private readonly ModelDataProvider _savingModelDataProvider;
        private readonly ModelDataProvider _previousModelDataProvider;
        private readonly ConsumptionCostCalculator _consumptionCostCalculator = new ConsumptionCostCalculator();

        private readonly SqlCeConnection _dbConnection;

        /// <summary>
        /// Instancia única
        /// </summary>
        public static ConsumptionManager Default
        {
            get { return _instance; }
        }

        /// <summary>
        /// Puerto donde escucha el servidor
        /// </summary>
        public int ListenPort
        {
            get { return Settings.Default.ListenPort; }
        }

        public ModelDataProvider SavingModelDataProvider
        {
            get { return _savingModelDataProvider; }
        }

        public ModelDataProvider PreviousModelDataProvider
        {
            get { return _previousModelDataProvider; }
        }

        public CommitedConsumptionDataProvider CommitedConsumptionDataProvider
        {
            get { return _commitedConsumptionDataProvider; }
        }

        public ConsumptionCostCalculator ConsumptionCostCalculator
        {
            get { return _consumptionCostCalculator; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private ConsumptionManager()
        {
            try
            {
                this._dbConnection = new SqlCeConnection(@"Data Source=" + Settings.Default.DataBasePath + ";Persist Security Info=False;");
                this._commitedConsumptionDataProvider = new CommitedConsumptionDataProvider(this._dbConnection){Log = LogManager.GetCurrentClassLogger()};
                this._savingModelDataProvider = new ModelDataProvider();
                this._previousModelDataProvider = new ModelDataProvider();

                if (DataReadersConfig.Instance == null)
                {
                    LogManager.GetCurrentClassLogger().Warn("No se ha encontrado la configuración de los lectores. Creando configuración Fake");
                    DataReadersConfig.CreateFakeConfig();
                    // ReSharper disable PossibleNullReferenceException
                    DataReadersConfig.Instance.Save();
                    // ReSharper restore PossibleNullReferenceException
                }

                LogManager.GetCurrentClassLogger().Trace("Cargando modelo anterior");
                if(!this._previousModelDataProvider.Load(Settings.Default.PreviousModelDirectoryPath))
                {
                    LogManager.GetCurrentClassLogger().Warn("No se ha cargado el modelo de consumo anterior.");
                }
                LogManager.GetCurrentClassLogger().Trace("Cargando modelo de ahorro");
                if(!this._savingModelDataProvider.Load(Settings.Default.SavingModelDirectoryPath))
                {
                    LogManager.GetCurrentClassLogger().Warn("No se ha cargado el modelo de ahorro.");
                }
                LogManager.GetCurrentClassLogger().Trace("Cargando archivo de costes de energía");
                if(!this.ConsumptionCostCalculator.Load(Settings.Default.PowerCostFilePath))
                {
                    LogManager.GetCurrentClassLogger().Warn("No se han cargado información sobre los costes energéticos. No se podrá estimar el coste del periodo ni el ahorro.");
                }

                this.ConsumptionCostCalculator.ConsumptionDataProvider = this.CommitedConsumptionDataProvider;
                this.ConsumptionCostCalculator.SavingModelDataProvider = this.SavingModelDataProvider;
                this.ConsumptionCostCalculator.PreviousModelDataProvider = this.PreviousModelDataProvider;

                foreach (DataReaderConfigItem configItem in DataReadersConfig.Instance.DataReadersList)
                {
                    DataReader dataReader = DataReader.Create(configItem.ClassName, configItem.SourceId,
                                                              configItem.ComPort, configItem.DeviceId,
                                                              Settings.Default.ReadCurrentPowerInterval,
                                                              Settings.Default.PollingQPowerInterval,
                                                              this._dbConnection);
                    dataReader.Log = LogManager.GetCurrentClassLogger();
                    this._dataReaderCollection.Add(dataReader.SourceId, dataReader);
                    dataReader.QDataRead += OnDataReaderQDataRead;
                }

                AlertsContainer.AutoSave = true;
            }catch(Exception ex)
            {
                LogManager.GetCurrentClassLogger().FatalException("No se puede iniciar ConsumptionManager", ex);
                throw;
            }
        }

        private void OnDataReaderQDataRead(object sender, DataReaderEventArgs e)
        {
            DataReader dataReader = sender as DataReader;

            if((dataReader != null) && (this._savingModelDataProvider.Models.ContainsKey(dataReader.SourceId)))
            {
                short hour = (short)e.TimeStamp.Hour;
                float maxQConsumption = this._savingModelDataProvider.Models[dataReader.SourceId]
                    .FetchDayModel(e.TimeStamp)
                    .GetHourlyValues(hour, (short)(hour + 1))[0] / 4f;

                if((dataReader.LastQDataReaded.Data != null) && (dataReader.LastQDataReaded.Data > maxQConsumption))
                {
                    AlertsContainer.Instance.AddAlert(dataReader.SourceId, e.TimeStamp);
                    dataReader.NotifyRemoteListeners(new NewAlert
                                                         {
                                                             SourceId = dataReader.SourceId,
                                                             TimeStamp = e.TimeStamp
                                                         });
                }
            }
        }

        /// <summary>
        /// Inicia los lectores
        /// </summary>
        public void Start()
        {
            LogManager.GetCurrentClassLogger().Info("********** INICIANDO EJECUCIÓN **********");

            LogManager.GetCurrentClassLogger().Trace("Iniciando gestor de licencia");
            LicensingManager.Instance.ActivationBroker = new ManualActivationBroker();
            LicensingManager.Instance.LicenseCode = Settings.Default.LicenseCode;
            LicensingManager.Instance.Key = Resources.publicKey;

            LicenseStatus licenseStatus = LicensingManager.Instance.ValidateStoredActivationCode();
            LogManager.GetCurrentClassLogger().Trace(string.Format("Estado actual de la licencia: {0}", licenseStatus));
            if((licenseStatus != LicenseStatus.Licensed) && File.Exists(Settings.Default.LicenseFilePath))
            {
                licenseStatus =
                    LicensingManager.Instance.ValidateActivationCode(File.ReadAllText(Settings.Default.LicenseFilePath));
                LogManager.GetCurrentClassLogger().Trace(string.Format("Probando licencia con fichero '{0}', estado: {1}", Settings.Default.LicenseFilePath, licenseStatus));
            }

            if(licenseStatus == LicenseStatus.Licensed)
            {
                LogManager.GetCurrentClassLogger().Info(string.Format("Licencia correcta: licencia = '{0}'", Settings.Default.LicenseCode));

                LogManager.GetCurrentClassLogger().Trace("Abriendo conexión con base de datos en " +
                                                         Settings.Default.DataBasePath);
                this._dbConnection.Open();
                foreach (DataReader dataReader in _dataReaderCollection.Values)
                {
                    try
                    {
                        dataReader.Start();
                    }catch(Exception ex)
                    {
                        LogManager.GetCurrentClassLogger().ErrorException(string.Format("No se ha podido iniciar el lector de datos de tipo '{0}'",dataReader.GetType()), ex);
                    }
                }
                LogManager.GetCurrentClassLogger().Trace("Iniciando listener");
                this._listener.Start();
            }
            else
            {
                LogManager.GetCurrentClassLogger().Info(string.Format("Licencia no válida. ID de dispositivo: {0}", DeviceIdGetter.Instance.GetDeviceId().ToString("x")));
            }
        }

        /// <summary>
        /// Detiene los lectores
        /// </summary>
        public void Stop()
        {
            LogManager.GetCurrentClassLogger().Trace("Deteniendo Listener");
            this._listener.Stop();
            foreach (DataReader dataReader in _dataReaderCollection.Values)
            {
                dataReader.Stop();
            }
            LogManager.GetCurrentClassLogger().Trace("Cerrando conexión con base de datos");
            this._dbConnection.Close();
            LogManager.GetCurrentClassLogger().Info("********** FINALIZANDO EJECUCIÓN **********");
        }

        /// <summary>
        /// Obtiene los nombres de las fuentes y sus magnitudes
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string[]> GetSourcesAndMagnitudes()
        {
            Dictionary<string, string []> magnitudesBySources = new Dictionary<string, string[]>(this._dataReaderCollection.Keys.Count);
            foreach(string source in this._dataReaderCollection.Keys)
            {
                magnitudesBySources.Add(source, this._dataReaderCollection[source].Magnitudes);
            }

            return magnitudesBySources;
        }

        /// <summary>
        /// Registra un elemento para que se le envíen los datos de consumo actual
        /// </summary>
        /// <param name="ipAddress">IP del cliente</param>
        /// <param name="port">Puerto</param>
        /// <param name="source">Identificador de la fuente de datos</param>
        public void RegisterListener(IPAddress ipAddress, int port, string source)
        {
            LogManager.GetCurrentClassLogger().Trace(string.Format("Petición de registro de {0} en puerto {1} para el punto {2}", ipAddress, port, source));

            if (this._dataReaderCollection.ContainsKey(source))
            {
                this._dataReaderCollection[source].RegisterListerner(new IPEndPoint(ipAddress, port));
            }
        }

        /// <summary>
        /// Elimina el registro del observador
        /// </summary>
        /// <param name="ipAddress">IP del cliente</param>
        /// <param name="source">Identificador de la fuente de datos</param>
        public void UnregisterListener(IPAddress ipAddress, string source)
        {
            LogManager.GetCurrentClassLogger().Trace(string.Format("Petición de desregistro de {0} para el punto {1}", ipAddress, source));

            if (this._dataReaderCollection.ContainsKey(source))
            {
                this._dataReaderCollection[source].UnregisterListener(ipAddress);
            }
        }
    }
}
