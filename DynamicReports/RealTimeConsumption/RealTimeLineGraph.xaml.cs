using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DynamicReports.RealTimeConsumption
{
    /// <summary>
    /// Dibuja una gráfica lineal de un conjunto de datos en tiempo real 
    /// durante una ventana de tiempo.
    /// </summary>
    public partial class RealTimeLineGraph : UserControl
    {
        private int _maxWindowData = 100;
        private double _clientGraphCanvasHeight;
        private double _clientGraphCanvasWidth;

        private readonly Dictionary<string, Polyline> _polilynesDictionary = new Dictionary<string, Polyline>();
        private readonly List<RealTimeData> _dataCollection;

        /// <summary>
        /// Constructor
        /// </summary>
        public RealTimeLineGraph()
        {
            InitializeComponent();
            this._dataCollection = new List<RealTimeData>(_maxWindowData);
        }

        /// <summary>
        /// Número máximo de datos que se visualizan en la ventana
        /// </summary>
        public int MaxWindowData
        {
            get { return this._maxWindowData; }
            set
            {
                this._maxWindowData = value;
                this._dataCollection.Capacity = this._maxWindowData;
            }
        }

        /// <summary>
        /// Identificadores de las magnitudes de los datos
        /// </summary>
        public string[] DataMagnitudes
        {
            set
            {
                this._polilynesDictionary.Clear();
                foreach (string dataMagnitudeId in value)
                {
                    this._polilynesDictionary.Add(dataMagnitudeId, new Polyline
                                                                    {
                                                                        Stroke = this.GraphLine.Stroke,
                                                                        StrokeThickness = this.GraphLine.StrokeThickness,
                                                                        StrokeLineJoin = this.GraphLine.StrokeLineJoin,
                                                                        StrokeStartLineCap =
                                                                            this.GraphLine.StrokeStartLineCap,
                                                                        StrokeEndLineCap =
                                                                            this.GraphLine.StrokeEndLineCap
                                                                    });
                    this.GraphCanvas.Children.Add(this._polilynesDictionary[dataMagnitudeId]);
                }
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            this.GraphAxis.Points.Clear();
            this.GraphAxis.Points.Add(new Point { X = 0, Y = 0 });
            this.GraphAxis.Points.Add(new Point { X = 0, Y = this.GraphCanvas.ActualHeight });
            this.GraphAxis.Points.Add(new Point { X = this.GraphCanvas.ActualWidth, Y = this.GraphCanvas.ActualHeight });

            this._clientGraphCanvasHeight = this.GraphCanvas.ActualHeight;
            this._clientGraphCanvasWidth = this.GraphCanvas.ActualWidth;
        }

        /// <summary>
        /// Hace que se ajuste automáticamente al tamaño del padre
        /// </summary>
        /// <param name="oldParent"></param>
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            if(this.Parent != null)
            {
                this.Width = double.NaN;
                this.Height = double.NaN;
                this.GraphLine.Points.Clear();
            }
        }

        private void InvokeIfRequired(Delegate method, params object []args)
        {
            if(this.Dispatcher.CheckAccess())
            {
                method.Method.Invoke(this, args);
            }else
            {
                this.Dispatcher.Invoke(method, args);
            }
        }

        private void Update()
        {
            this.InvokeIfRequired( new Action(() =>
                                       {
                                           double offset = this._clientGraphCanvasWidth/
                                                           (this._maxWindowData - 1);
                                           double maxData = 0;

                                           //Buscar el mayor valor
                                           foreach (RealTimeData data in _dataCollection)
                                           {
                                               double maxValueInData = data.MaxValue();
                                               if (maxValueInData > maxData)
                                               {
                                                   maxData = maxValueInData;
                                               }
                                           }

                                           this.MaxValueLabel.Content = maxData.ToString("N2");

                                           foreach (string dataMagnitudeId in this._polilynesDictionary.Keys)
                                           {
                                               this.UpdateMagnitude(dataMagnitudeId, maxData, offset);
                                           }
                                       }));
        }

        private void UpdateMagnitude(string dataMagnitudeId, double maxData, double offset)
        {
            double x = 0;

            this._polilynesDictionary[dataMagnitudeId].Points.Clear();
            foreach (RealTimeData data in _dataCollection)
            {
                Point dataPoint = new Point { X = x, Y = this.ConvertToYCoordinate(data.DataCollection[dataMagnitudeId], maxData) };
                this._polilynesDictionary[dataMagnitudeId].Points.Add(dataPoint);
                x += offset;
            }

            //Canvas.SetTop(this.LastPoint, this.ConvertToYCoordinate(this._dataCollection[this._dataCollection.Count - 1], maxData) - this.LastPoint.ActualHeight / 2);
        }

        /// <summary>
        /// Asigna una brocha a la línea que representa una fuente de datos
        /// </summary>
        /// <param name="dataMagnitudeId">Identificador de la fuente de datos</param>
        /// <param name="brush">Brocha</param>
        public void SetBrush(string dataMagnitudeId, Brush brush)
        {
            if (this._polilynesDictionary.ContainsKey(dataMagnitudeId))
            {
                this._polilynesDictionary[dataMagnitudeId].Stroke = brush;
            }
        }

        /// <summary>
        /// Proporciona la brocha que se utiliza para una fuente de datos
        /// o null si la fuente de datos no existe.
        /// </summary>
        /// <param name="dataMagnitudeId">Identificador de la fuente de datos</param>
        /// <returns></returns>
        public Brush GetBrush(string dataMagnitudeId)
        {
            return (this._polilynesDictionary.ContainsKey(dataMagnitudeId) ? this._polilynesDictionary[dataMagnitudeId].Stroke : null);
        }

        /// <summary>
        /// Añade un dato a la gráfica
        /// </summary>
        ///<param name="data">Datos</param>
        public void PumpData(RealTimeData data)
        {
            if(this._dataCollection.Count >= _maxWindowData)
            {
                this._dataCollection.RemoveAt(0);
            }

            this._dataCollection.Add(data);
            if (this._dataCollection.Count > 1) //Tiene que haber al menos 2 elementos para dibujar una línea
            {
                this.Update();
            }
        }

        /// <summary>
        /// Fuerza a que se repinte el control
        /// </summary>
        public void Refresh()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        }

        private double ConvertToYCoordinate(double data, double maxData)
        {
            return this._clientGraphCanvasHeight - (data * this._clientGraphCanvasHeight / maxData);
        }
    }
}
