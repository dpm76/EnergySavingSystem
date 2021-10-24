using System.Windows.Controls;

namespace EnergySavingManager
{
    /// <summary>
    /// Visor de coste del consumo
    /// </summary>
    public partial class CostViewer : UserControl, ICostViewer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CostViewer()
        {
            InitializeComponent();
        }

        protected override void OnVisualParentChanged(System.Windows.DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            if(this.Parent!=null)
            {
                this.Width = double.NaN;
                this.Height = double.NaN;
            }
        }

        public string SourceId { get; set; }

        public float CommitedPowerCost
        {
            set { this.UpdateLabel(this.CommitedPowerCostLabel, string.Format("{0} €", value)); }
        }

        public float EstimatedPowerCost
        {
            set { this.UpdateLabel(this.EstimatedPowerCostLabel, string.Format("{0} €", value)); }
        }

        public float EstimatedPowerCostSaved
        {
            set { this.UpdateLabel(this.EstimatedPowerCostSavedLabel, string.Format("{0} €", value)); }
        }

        private void UpdateLabel(Label label, string text)
        {
            if(this.Dispatcher.CheckAccess())
            {
                UpdateLabelInvoked(label, text);
            }else
            {
                this.Dispatcher.Invoke(new UpdateLabelDelegate(UpdateLabelInvoked), new object[] {label, text});
            }
        }

        private delegate void UpdateLabelDelegate(Label label, string text);

        private static void UpdateLabelInvoked(Label label, string text)
        {
            label.Content = text;
        }

    }
}
