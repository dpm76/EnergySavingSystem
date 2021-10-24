using System;
using System.Windows.Controls;

namespace EnergySavingManager.Alerts
{
    /// <summary>
    /// Elemento de alerta
    /// </summary>
    public partial class AlertItem : UserControl
    {
        public event EventHandler ClosePressed;
        public event EventHandler GoPressed;

        private string _sourceId;
        private DateTime _timeStamp;

        /// <summary>
        /// Constructor
        /// </summary>
        public AlertItem()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Fuente de datos que ha generado la alerta
        /// </summary>
        public string SourceId
        {
            get { return _sourceId; }
            set
            {
                _sourceId = value;
                this.SourceIdLabel.Content = value;
            }
        }

        /// <summary>
        /// Marca horaria del aviso
        /// </summary>
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set 
            {
                _timeStamp = value;
                this.TimeStampLabel.Content = value.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            }
        }

        protected override void OnVisualParentChanged(System.Windows.DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            if(this.Parent != null)
            {
                this.Width = this.Height = double.NaN;
            }
        }

        private void OnEvent(EventHandler eventHandler)
        {
            if(eventHandler != null)
            {
                eventHandler(this, EventArgs.Empty);
            }
        }

        private void OnCloseButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            this.OnEvent(this.ClosePressed);
        }

        private void OnGoButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            this.OnEvent(this.GoPressed);
        }
    }
}
