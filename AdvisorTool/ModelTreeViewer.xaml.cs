using System;
using System.Windows.Controls;
using AdvisorTool.ViewModel;
using ConsumptionModelling.Model;

namespace AdvisorTool
{
    /// <summary>
    /// Lógica de interacción para ModelEditor.xaml
    /// </summary>
    public partial class ModelTreeViewer : UserControl
    {
        public static YearLevel YearLevel;

        public event EventHandler<ModelEventArgs> Selected;

        public ModelTreeViewer()
        {
            InitializeComponent();

            this.DataContext = new ViewModelRoot(YearLevel);
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

        private void OnSelected(IModel model)
        {
            EventHandler<ModelEventArgs> temp = Selected;
            if(temp != null)
            {
                temp(this, new ModelEventArgs(model));
            }
        }

        private void OnTextBlockMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBlock textBlock = (TextBlock) sender;
            this.OnSelected((IModel) textBlock.Tag);
        }
    }
}
