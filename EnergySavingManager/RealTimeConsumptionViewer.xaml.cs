using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using DynamicReports.BarChart;
using DynamicReports.RealTimeConsumption;

namespace EnergySavingManager
{
    /// <summary>
    /// Agrupa el consumo energético en tiempo real y el consumo
    /// Q-horario
    /// </summary>
    public partial class RealTimeConsumptionViewer : UserControl, IRealTimeConsumptionViewer
    {
        private string _sourceId = string.Empty;
        private int _maxQData = 16;
        private readonly List<float?> _qDataCollection;
        private readonly List<float> _qSavingDataCollection;

        public RealTimeConsumptionViewer()
        {
            InitializeComponent();
            this._qDataCollection = new List<float?>(this._maxQData);
            this._qSavingDataCollection = new List<float>(this._maxQData);
            this.BarGraph.BarsOpacity = 0.7f;
            this.BarGraph.AddShadowFeatures(Manager.SAVING_LAYER_NAME,
                                            new ItemFeatures
                                                {
                                                    Fill = Brushes.LightGreen,
                                                    Stroke = Brushes.LimeGreen,
                                                    Opacity = 1f,
                                                    StrokeThickness = 2d,
                                                    StrokeLineJoin = PenLineJoin.Bevel
                                                });
            this.Clear();
        }

        protected override void OnVisualParentChanged(System.Windows.DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            if(this.Parent != null)
            {
                this.Height = this.Width = double.NaN;
            }
        }

        /// <summary>
        /// Identificador de la fuente
        /// </summary>
        public string SourceId
        {
            get { return this._sourceId; }
            set
            {
                this._sourceId = value;
                this.SourceName.Content = value;
            }
        }

        /// <summary>
        /// Número máximo de datos cuarto-horarios mostrados
        /// </summary>
        public int MaxQData
        {
            get { return _maxQData; }
            set 
            {
                this._maxQData = value;
                this._qDataCollection.Capacity = value;
                this._qSavingDataCollection.Capacity = value;
                for (int i = this._qDataCollection.Count; i < this._maxQData; i++)
                {
                    this._qDataCollection.Add(0f);
                    this._qSavingDataCollection.Add(0f);
                }
            }
        }

        /// <summary>
        /// Añade una colección de datos de un momento concreto
        /// </summary>
        /// <param name="dataColection">Colección de datos</param>
        /// <param name="timeStamp">Momento de lectura</param>
        public void PumpData(Dictionary<string, float> dataColection, DateTime timeStamp)
        {
            this.LineGraph.PumpData(new RealTimeData
                                        {
                                            DataCollection = dataColection,
                                            TimeStamp = timeStamp
                                        });
        }

        /// <summary>
        /// Muestra un dato cuarto-horario
        /// </summary>
        /// <param name="data">dato</param>
        /// <param name="savingData">dato de consumo de ahorro</param>
        /// <param name="timeStamp">Marca de hora</param>
        public void ShowQData(float? data, float savingData, DateTime timeStamp)
        {
            if(this.Dispatcher.CheckAccess())
            {
                this.ShowQDataInvoked(data, savingData, timeStamp);
            }else
            {
                this.Dispatcher.Invoke(new ShowQDataDelegate(this.ShowQDataInvoked), new object[] {data, savingData, timeStamp});
            }
        }

        private delegate void ShowQDataDelegate(float? data, float savingData, DateTime timeStamp);

        /// <summary>
        /// Muestra un dato cuarto-horario
        /// </summary>
        /// <param name="data">dato de consumo</param>
        /// <param name="savingData">dato de consumo de ahorro</param>
        /// <param name="timeStamp">Marca de hora</param>
        private void ShowQDataInvoked(float? data, float savingData, DateTime timeStamp)
        {
            this.BarGraph.Clear();
            lock (this._qDataCollection)
            {
                if (this._qDataCollection.Count == this._maxQData)
                {
                    this._qDataCollection.RemoveAt(0);
                    this._qSavingDataCollection.RemoveAt(0);
                }
                this._qDataCollection.Add(data);
                this._qSavingDataCollection.Add(savingData);
                this.BarGraph.AddShadow(Manager.SAVING_LAYER_NAME, this._qSavingDataCollection);
                this.BarGraph.AddBars(this._qDataCollection);
                this.BarGraph.MarkExceedingBars(Manager.SAVING_LAYER_NAME, new ItemFeatures
                                                                 {
                                                                     Fill = Brushes.LightSalmon,
                                                                     Opacity = 0.8f,
                                                                     Stroke = Brushes.Red,
                                                                     StrokeLineJoin = PenLineJoin.Miter,
                                                                     StrokeThickness = 2d
                                                                 });
            }
        }

        /// <summary>
        /// Borra todos los datos mostrados
        /// </summary>
        public void Clear()
        {
            lock (this._qDataCollection)
            {
                this._qDataCollection.Clear();
                this._qDataCollection.Clear();
                float?[] data = new float?[this._maxQData];
                float[] savingData = new float[this._maxQData];
                Array.Clear(data, 0, this._maxQData);
                Array.Clear(savingData, 0, this._maxQData);
                this._qDataCollection.AddRange(data);
                this._qSavingDataCollection.AddRange(savingData);
            }
        }
    }
}
