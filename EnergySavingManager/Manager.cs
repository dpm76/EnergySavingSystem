using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using Communication;
using Communication.Messages;
using DynamicReports.BarChart;
using EnergySavingManager.Alerts;
using EnergySavingManager.Properties;
using NLog;

namespace EnergySavingManager
{
    /// <summary>
    /// Clase encargada de coordinar la aplicación
    /// </summary>
    public class Manager:IRealTimeMessageDispatcher
    {
        public const string SAVING_LAYER_NAME = "savings";

        private SpanPicker _spanPicker;
        private BarGraph _historicConsumptionViewer;

        private ItemFeatures _markedBarsFeatures;

        /// <summary>
        /// Instancia única
        /// </summary>
        public static Manager Default = new Manager();

        private readonly RealTimeListener _listener = new RealTimeListener(Settings.Default.RealTimeDataPort);

        private readonly Dictionary<string, IRealTimeConsumptionViewer> _realTimeConsumptionViewers = new Dictionary<string, IRealTimeConsumptionViewer>();

        private readonly Dictionary<string, ICostViewer> _costViewers = new Dictionary<string, ICostViewer>();

        private IRealTimeConsumptionViewer _selectedConsumptionViewer;

        private NotificationsContainer _alertsContainer;

        public SpanPicker SpanPicker
        {
            set 
            {
                this._spanPicker = value;
                this._spanPicker.SelectionChanged += OnSpanPickerSelectionChanged;
                this._spanPicker.TimeRangeChanged += OnSpanPickerTimeRangeChanged;
            }
        }

        public BarGraph HistoricConsumptionViewer
        {
            set { this._historicConsumptionViewer = value; }
        }

        private void OnSpanPickerTimeRangeChanged(object sender, EventArgs e)
        {
            //TODO
            //throw new NotImplementedException();
            MessageBox.Show("Sorry, this functionallity is not implemented yet.","Not implemented", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnSpanPickerSelectionChanged(object sender, EventArgs e)
        {
            DateTime since = this._spanPicker.SelectionSince;
            DateTime until = this._spanPicker.SelectionUntil;
            string sourceId = this._selectedConsumptionViewer.SourceId;

            this.ShowHistoricConsumption(sourceId, since, until);
        }

        private void ShowHistoricConsumption(string sourceId, DateTime since, DateTime until)
        {
            List<float?> qData = this.GetHistoricConsumption(sourceId, since, until);
            List<float> modelData = this.GetModelData(sourceId, since, until, true);
            List<float> savingData = (modelData != null) ? ConvertHourToQHourData(modelData) : null;

            this._historicConsumptionViewer.Clear();
            if (savingData != null)
            {
                this._historicConsumptionViewer.AddShadow(SAVING_LAYER_NAME, savingData);
            }
            this._historicConsumptionViewer.AddBars(qData);
            if (savingData != null)
            {
                this._historicConsumptionViewer.MarkExceedingBars(SAVING_LAYER_NAME, _markedBarsFeatures);
            }
        }

        private static List<float> ConvertHourToQHourData(IList<float> hourlyData)
        {
            List<float> qHourData = new List<float>(hourlyData.Count*4);
            foreach (float value in hourlyData)
            {
                float qValue = value/4f;
                for (int i = 0; i < 4; i++)
                {
                    qHourData.Add(qValue);
                }
            }

            return qHourData;
        }

        /// <summary>
        /// Visores de consumo en tiempo real
        /// </summary>
        public Dictionary<string, IRealTimeConsumptionViewer> RealTimeConsumptionViewers
        {
            get { return this._realTimeConsumptionViewers; }
        }

        public IRealTimeConsumptionViewer SelectedConsumptionViewer
        {
            set { _selectedConsumptionViewer = value; }
        }

        public ItemFeatures MarkedBarsFeatures
        {
            set { _markedBarsFeatures = value; }
        }

        public NotificationsContainer AlertsContainer
        {
            set
            {
                this._alertsContainer = value;
                this._alertsContainer.AlertDeleting += OnAlertsContainerAlertDeleting;
                this._alertsContainer.AlertSelected += OnAlertsContainerAlertSelected;
            }
        }

        public Dictionary<string, ICostViewer> CostViewers
        {
            get { return _costViewers; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private Manager()
        {
            this._listener.MessageDispatcher = this;
        }

        public void Start()
        {
            LogManager.GetCurrentClassLogger().Info("****** INICIANDO ******");
            this._listener.Start();
            this._alertsContainer.Init();
            this.UpdateConsumptionViewer();
        }

        public void Stop()
        {
            this._listener.Stop();
            this.UnregisterAll();
            LogManager.GetCurrentClassLogger().Info("****** FINALIZANDO ******");
        }

        private void OnAlertsContainerAlertSelected(object sender, AlertEventArgs e)
        {
            TimeSpan timeSpan = new TimeSpan(this._spanPicker.SelectionUntil.Subtract(this._spanPicker.SelectionSince).Ticks/2);
            DateTime since = e.TimeStamp.Subtract(timeSpan);
            DateTime until = e.TimeStamp.Add(timeSpan);
            this._selectedConsumptionViewer.SourceId = e.SourceId;
            this._spanPicker.SetSelectedSpanTime(since, until);
        }

        private void OnAlertsContainerAlertDeleting(object sender, AlertEventArgs e)
        {
            this.DeleteAlert(e.SourceId, e.TimeStamp);
        }

        private void UpdateConsumptionViewer()
        {
            if(this._selectedConsumptionViewer != null)
            {
                DateTime now = DateTime.UtcNow;
                DateTime since = now.Subtract(new TimeSpan(0, this._selectedConsumptionViewer.MaxQData*15, 0));

                List<float?> data = this.GetHistoricConsumption(this._selectedConsumptionViewer.SourceId, since, now);
                List<float> modelData = this.GetModelData(this._selectedConsumptionViewer.SourceId, since, now, true);
                DateTime timeStamp = since;
                int dataIndex = since.Minute / 15;
                foreach (float? value in data)
                {
                    this._selectedConsumptionViewer.ShowQData(value, modelData[dataIndex/4]/4f, timeStamp);
                    timeStamp = timeStamp.AddMinutes(15);
                    ++dataIndex;
                }
            }
        }

        #region Message Dispatching

        /// <summary>
        /// Procesa un mensaje
        /// </summary>
        /// <param name="message">Mensaje</param>
        public void Dispatch(IMessage message)
        {
            if(message is ConsumptionDataMessage)
            {
                this.DispatchConsumptionDataMessage(message as ConsumptionDataMessage);
            }else if(message is QHourPowerConsumptionRealTimeData)
            {
                this.DispatchQHourPowerConsumptionDataMessage(message as QHourPowerConsumptionRealTimeData);
            }else if(message is NewAlert)
            {
                this.DispatchNewAlert(message as NewAlert);
            }
        }

        private void DispatchNewAlert(NewAlert message)
        {
            this._alertsContainer.AddAlert(message.SourceId, message.TimeStamp);
        }

        private void DispatchConsumptionDataMessage(ConsumptionDataMessage message)
        {
            if ((this.RealTimeConsumptionViewers!=null) && (this.RealTimeConsumptionViewers.ContainsKey(message.Source)))
            {
                this.RealTimeConsumptionViewers[message.Source].PumpData(message.EnergyData, message.ReadTimeStamp);
            }
            if((this._selectedConsumptionViewer!=null)&&(this._selectedConsumptionViewer.SourceId.Equals(message.Source)))
            {
                this._selectedConsumptionViewer.PumpData(message.EnergyData, message.ReadTimeStamp);
                if (message.EnergyData.ContainsKey("V") && message.EnergyData.ContainsKey("W"))
                {
                    LogManager.GetCurrentClassLogger().Trace("{0} W; {1} V", message.EnergyData["W"],
                                                                  message.EnergyData["V"]);
                }
            }
        }

        private void DispatchQHourPowerConsumptionDataMessage(QHourPowerConsumptionRealTimeData message)
        {
            LogManager.GetCurrentClassLogger().Trace(message.TimeStamp + ((message.TimeStamp.Kind == DateTimeKind.Utc) ? "Z" : "!"));

            //Actualizar visor de consumo
            if ((this._selectedConsumptionViewer != null) && (this._selectedConsumptionViewer.SourceId.Equals(message.SourceId)))
            {
                List<float> data = this.GetModelData(message.SourceId, message.TimeStamp.Subtract(new TimeSpan(0, 15, 0)), message.TimeStamp, true);
                this._selectedConsumptionViewer.ShowQData(message.Data, ((data != null) && (data.Count > 0)) ? (data[0] / 4f) : 0f, message.TimeStamp);
            }

            //Actualizar visor de costes
            if((this._costViewers != null) && (this._costViewers.ContainsKey(message.SourceId)))
            {
                InvoicePeriodProvider.Period period = InvoicePeriodProvider.GetPeriod(DateTime.Today);
                this.UpdateCostViewer(this._costViewers[message.SourceId], period.Since, period.Until);
            }
        }

        #endregion

        #region No real-time messaging

        /// <summary>
        /// Obtiene los nombres de las fuentes de consumo.
        /// Lo obtiene de la configuración. 
        /// Si no hay datos, pregunta al gestor de consumo.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,string[]> GetConsumptionSourcesAndMagnitudes()
        {
            Dictionary<string, string[]> magnitudesBySources;

            using (MessagingClient client = new MessagingClient(
                Settings.Default.ConsumptionListenerHost,
                Settings.Default.ConsumptionListenerPort))
            {
                magnitudesBySources = (Dictionary<string, string[]>)
                    client.Send(new GetAvailableConsumptionSourcesCommand(), GetConsumptionSourcesAndMagnitudesConversation);
            }

            return magnitudesBySources;
        }

        private static object GetConsumptionSourcesAndMagnitudesConversation(IMessage message, NetworkStream stream)
        {
            return ((AvailableConsumptionSourcesMessage)message).MagnitudesBySources;
        }

        /// <summary>
        /// Registra en el lector de consumo para recibir los datos
        /// </summary>
        /// <param name="sourceId">Identificador de la fuente</param>
        public void RegisterIntoConsumptionDataReader(string sourceId)
        {
            using (MessagingClient client = new MessagingClient(Settings.Default.ConsumptionListenerHost, Settings.Default.ConsumptionListenerPort))
            {
                client.Send(new RegisterConsumptionDataReaderListenerCommand
                                {ListenPort = Settings.Default.RealTimeDataPort, SourceId = sourceId});
            }
        }

        public ConsumptionCostInfo GetConsumptionCostInfo(GetConsumptionCosts command)
        {
            using (MessagingClient client = new MessagingClient(Settings.Default.ConsumptionListenerHost, Settings.Default.ConsumptionListenerPort))
            {
                return (ConsumptionCostInfo)client.Send(command, GetConsumptionInfoConversation);
            }
        }

        private static object GetConsumptionInfoConversation(IMessage message, NetworkStream stream)
        {
            return message;
        }

        /// <summary>
        /// Elimina el registro en el lector de consumo para dejar de recibir los datos
        /// </summary>
        /// <param name="sourceId">Identificador de la fuente</param>
        public void UnregisterIntoConsumptionDataReader(string sourceId)
        {
            using (MessagingClient client = new MessagingClient(Settings.Default.ConsumptionListenerHost, Settings.Default.ConsumptionListenerPort))
            {
                client.Send(new UnregisterConsumptionDataReaderListenerCommand {SourceId = sourceId});
            }
        }

        /// <summary>
        /// Elimina el registro para todas las fuentes de datos
        /// </summary>
        public void UnregisterAll()
        {
            foreach (string source in RealTimeConsumptionViewers.Keys)
            {
                this.UnregisterIntoConsumptionDataReader(source);
            }
        }

        public List<float> GetModelData(string sourceId, DateTime since, DateTime until, bool savingModel)
        {
            LogManager.GetCurrentClassLogger().Trace(string.Format("Obteniendo datos del modelo. sourceId = {0}; since = {1}; until = {2}; savingModel = {3}", sourceId, since, until, savingModel));

            List<float> data;
            using (MessagingClient client = new MessagingClient(Settings.Default.ConsumptionListenerHost, Settings.Default.ConsumptionListenerPort))
            {
                try
                {
                    NetworkStream stream = client.Send(new ModelDataQuery
                                                           {
                                                               SaveModel = savingModel,
                                                               Since = since,
                                                               SourceId = sourceId,
                                                               Until = until
                                                           });

                    ModelDataResponse response = (ModelDataResponse) MessageSerializer.Instance.FromStream(stream);
                    data = response.Data;
                }catch
                {
                    data = new List<float>(new []{0f});
                }
            }

            //Trace
            if (data != null)
            {
                StringBuilder dataStr = new StringBuilder();
                foreach (float f in data)
                {
                    dataStr.Append(f + "; ");
                }
                LogManager.GetCurrentClassLogger().Trace("Obtenido modelo: " + dataStr);
            }else
            {
                LogManager.GetCurrentClassLogger().Error("No se han devuelto datos del modelo. sourceId = {0}; since = {1}; until = {2}; savingModel = {3}", sourceId, since, until, savingModel);
            }
            return data;
        }

        public List<float?> GetHistoricConsumption(string sourceId, DateTime since, DateTime until)
        {
            LogManager.GetCurrentClassLogger().Trace("Obteniendo datos históricos. sourceId = {0}; since = {1}; until = {2}", sourceId, since, until);

            List<float?> data;
            try
            {
                using(MessagingClient client = 
                    new MessagingClient(
                        Settings.Default.ConsumptionListenerHost,
                        Settings.Default.ConsumptionListenerPort))
                {

                    NetworkStream stream = client.Send(new QHourConsumptionHistoricDataQuery
                                                           {
                                                               Since = since,
                                                               SourceId = sourceId,
                                                               Until = until
                                                           });

                    QHourConsumptionHistoricDataResponse response =
                        (QHourConsumptionHistoricDataResponse) MessageSerializer.Instance.FromStream(stream);
                    data = response.Data;

                }
            }catch
            {
                data = new List<float?>(new[] { (float?)0f });
            }

            //Trace
            StringBuilder dataStr = new StringBuilder();
            foreach (float? f in data)
            {
                dataStr.Append((f.HasValue ? f.Value.ToString() : "null") + "; ");
            }
            LogManager.GetCurrentClassLogger().Trace("Obtenidos datos históricos: " + dataStr);

            return data;
        }

        public Dictionary<string, List<DateTime>> GetAllStoredAlerts()
        {
            Dictionary<string, List<DateTime>> alerts = new Dictionary<string, List<DateTime>>();
            using(MessagingClient client = new MessagingClient(Settings.Default.ConsumptionListenerHost, Settings.Default.ConsumptionListenerPort))
            {
                using(NetworkStream stream = client.Send(new GetAlerts{All = true}))
                {
                    SourceAlerts message = (SourceAlerts)MessageSerializer.Instance.FromStream(stream);
                    while(!message.Empty)
                    {
                        alerts.Add(message.SourceId, message.AlertTimeStamps);
                        message = (SourceAlerts)MessageSerializer.Instance.FromStream(stream);
                    }
                }
            }

            return alerts;
        }

        public List<DateTime> GetSourceStoredAlerts(string sourceId)
        {
            List<DateTime> alerts = new List<DateTime>();
            using (MessagingClient client = new MessagingClient(Settings.Default.ConsumptionListenerHost, Settings.Default.ConsumptionListenerPort))
            {
                using (NetworkStream stream = client.Send(new GetAlerts { All = false, SourceId = sourceId}))
                {
                    SourceAlerts message = (SourceAlerts)MessageSerializer.Instance.FromStream(stream);
                    if(!message.Empty)
                    {
                        alerts = message.AlertTimeStamps;
                    }
                }
            }

            return alerts;
        }

        public void DeleteAlert(string sourceId, DateTime timeStamp)
        {
            using (MessagingClient client = new MessagingClient(Settings.Default.ConsumptionListenerHost, Settings.Default.ConsumptionListenerPort))
            {
                client.Send(new DeleteAlert {SourceId = sourceId, TimeStamp = timeStamp});
            }
        }

        public void DeleteSourceAlerts(string sourceId)
        {
            using (MessagingClient client = new MessagingClient(Settings.Default.ConsumptionListenerHost, Settings.Default.ConsumptionListenerPort))
            {
                client.Send(new DeleteSourceAlerts {All = false, SourceId = sourceId});
            }
        }

        public void DeleteAllAlerts()
        {
            using (MessagingClient client = new MessagingClient(Settings.Default.ConsumptionListenerHost, Settings.Default.ConsumptionListenerPort))
            {
                client.Send(new DeleteSourceAlerts { All = true, SourceId = string.Empty});
            }
        }
        #endregion

        private void UpdateCostViewer(ICostViewer costViewer, DateTime since, DateTime until)
        {
            ConsumptionCostInfo costInfo = this.GetConsumptionCostInfo(new GetConsumptionCosts
                                            {SourceId = costViewer.SourceId, Since = since, Until = until});

            costViewer.CommitedPowerCost = costInfo.CommitedPowerCost;
            costViewer.EstimatedPowerCost = costInfo.EstimatedPowerCost;
            costViewer.EstimatedPowerCostSaved = costInfo.EstimatedPowerCostSaved;
        }
    }
}
