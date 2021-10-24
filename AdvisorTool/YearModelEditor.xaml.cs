using System.Windows.Controls;

namespace AdvisorTool
{
    /// <summary>
    /// Lógica de interacción para YearModelEditor.xaml
    /// </summary>
    public partial class YearModelEditor : UserControl
    {
        public YearModelEditor()
        {
            InitializeComponent();
        }

        protected override void OnVisualParentChanged(System.Windows.DependencyObject oldParent)
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
