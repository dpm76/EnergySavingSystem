using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace EnergySavingManager.Alerts
{
    /// <summary>
    /// Contenedor de elementos de alerta
    /// </summary>
    public partial class NotificationsContainer : UserControl
    {
        public event EventHandler<AlertEventArgs> AlertSelected;
        public event EventHandler<AlertEventArgs> AlertDeleting;

        /// <summary>
        /// Constructor
        /// </summary>
        public NotificationsContainer()
        {
            InitializeComponent();
        }

        public void Init()
        {
            this.Clear();
            Dictionary<string, List<DateTime>> alerts = Manager.Default.GetAllStoredAlerts();
            foreach (string sourceId in alerts.Keys)
            {
                foreach (DateTime timeStamp in alerts[sourceId])
                {
                    this.AddAlert(sourceId, timeStamp);
                }
            }
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            if(this.Parent != null)
            {
                this.Width = this.Height = double.NaN;
            }
        }

        /// <summary>
        /// Añade una alerta
        /// </summary>
        /// <param name="sourceId">Id de fuente</param>
        /// <param name="timeStamp">Marca horaria del evento (UTC)</param>
        public void AddAlert(string sourceId, DateTime timeStamp)
        {
            if(this.CheckAccess())
            {
                this.AddAlertInvoked(sourceId, timeStamp);
            }else
            {
                this.Dispatcher.Invoke(new AddAlertDelegate(AddAlertInvoked), new object[] {sourceId, timeStamp});
            }
        }

        private delegate void AddAlertDelegate(string sourceId, DateTime timeStamp);
        private void AddAlertInvoked(string sourceId, DateTime timeStamp)
        {
            AlertItem alertItem = new AlertItem
            {
                SourceId = sourceId,
                TimeStamp = timeStamp.ToUniversalTime()
            };
            alertItem.GoPressed += OnAlertItemGoPressed;
            alertItem.ClosePressed += OnAlertItemClosePressed;
            this.AlertsStackPanel.Children.Add(alertItem);
        }

        private void OnAlertItemClosePressed(object sender, EventArgs e)
        {
            AlertItem alertItem = (AlertItem) sender;
            this.OnAlertEvent(this.AlertDeleting, new AlertEventArgs(alertItem.SourceId, alertItem.TimeStamp));
            this.RemoveAlert(alertItem);
        }

        private void OnAlertItemGoPressed(object sender, EventArgs e)
        {
            AlertItem alertItem = (AlertItem) sender;
            this.OnAlertEvent(this.AlertSelected, new AlertEventArgs(alertItem.SourceId, alertItem.TimeStamp));
        }

        private void RemoveAlert(AlertItem alertItem)
        {
            alertItem.ClosePressed -= OnAlertItemClosePressed;
            alertItem.GoPressed -= OnAlertItemGoPressed;

            this.AlertsStackPanel.Children.Remove(alertItem);
        }

        private void OnAlertEvent(EventHandler<AlertEventArgs> eventHandler, AlertEventArgs eventArgs)
        {
            if(eventHandler != null)
            {
                eventHandler(this, eventArgs);
            }
        }

        /// <summary>
        /// Elimina todas las alertas
        /// </summary>
        public void Clear()
        {
            while(this.AlertsStackPanel.Children.Count > 0)
            {
                this.RemoveAlert((AlertItem)this.AlertsStackPanel.Children[0]);
            }
        }
    }
}
