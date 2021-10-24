using System;
using System.Windows;
using System.Windows.Controls;

namespace EnergySavingManager
{
    /// <summary>
    /// Lógica de interacción para ConsumptionBarGraph.xaml
    /// </summary>
    [Obsolete]
    public partial class ConsumptionBarGraph : UserControl
    {
        public ConsumptionBarGraph()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Hace que se ajuste automáticamente al tamaño del padre
        /// </summary>
        /// <param name="oldParent"></param>
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            if (this.Parent != null)
            {
                this.Width = double.NaN;
                this.Height = double.NaN;
            }
        }
    }
}
