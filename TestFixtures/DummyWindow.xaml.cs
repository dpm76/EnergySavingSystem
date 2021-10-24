using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TestFixtures
{
    /// <summary>
    /// Ventana que acoge los controles para probarlos
    /// </summary>
    public partial class DummyWindow : Window
    {
        private readonly BackgroundWorker _bgWorker = new BackgroundWorker();

        private object _result;

        /// <summary>
        /// Delegado del método que se procesará en la prueba
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public delegate object ProcessDelegate(object args);

        /// <summary>
        /// Proceso que se ejecutará durante la prueba
        /// </summary>
        public ProcessDelegate Process { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DummyWindow()
        {
            InitializeComponent();
            this._bgWorker.DoWork += OnWorkStart;
            this._bgWorker.RunWorkerCompleted += OnWorkFinished;
        }

        private void OnWorkFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            this._result = e.Result;
            if (this.Dispatcher.CheckAccess())
            {
                this.CloseInvoked();
            }else
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new CloseDelegate(this.CloseInvoked));
            }
        }

        private delegate void CloseDelegate();
        private void CloseInvoked()
        {
            this.Close();
        }

        private void OnWorkStart(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(3000);
            e.Result = this.Process(e.Argument);
        }

        /// <summary>
        /// Asigna un control a la ventana de pruebas
        /// </summary>
        public UserControl TestingControl
        {
            set
            {
                this.Width = value.Width;
                this.Height = value.Height + 50;
                this.InnerPanel.Children.Add(value);
            }
        }

        /// <summary>
        /// Ejecuta el proceso de prueba
        /// </summary>
        /// <param name="args">Parámatros del proceso de prueba</param>
        /// <returns></returns>
        public object DoProcess(object args)
        {
            this._bgWorker.RunWorkerAsync(args);
            this.ShowDialog();
            return this._result;
        }

        /// <summary>
        /// Ejecuta el proceso de prueba
        /// </summary>
        public object DoProcess()
        {
            return this.DoProcess(null);
        }
    }
}
