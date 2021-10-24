using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DynamicReports.BarChart
{
    /// <summary>
    /// Control para representar datos en forma de gráfico de barras
    /// Las barras pueden ser modificables por el usuario
    /// </summary>
    public partial class BarGraph : UserControl
    {
        private readonly List<Bar> _barsCollection = new List<Bar>();
        private readonly Dictionary<string, IList<float>> _shadowsDataCollection = new Dictionary<string, IList<float>>(2);
        private readonly Dictionary<string, Polygon> _shadowsShapesCollection = new Dictionary<string, Polygon>(2);
        private readonly Dictionary<string, ItemFeatures> _shadowFeaturesCollection = new Dictionary<string, ItemFeatures>(2);

        private float _chartMaxValue;
        private float _barsOpacity = 1f;
        private float _zoomStep = .05f;
        private Bar _maxValueBar;

        /// <summary>
        /// Constructor
        /// </summary>
        public BarGraph()
        {
            InitializeComponent();
        }

        private float ChartMaxValue
        {
            get { return _chartMaxValue; }
            set
            {
                _chartMaxValue = value;
                this.MaxValueLabel.Content = value;
            }
        }

        /// <summary>
        /// Opacidad de las barras
        /// </summary>
        public float BarsOpacity
        {
            get { return this._barsOpacity; }
            set { this._barsOpacity = value; }
        }
        
        /// <summary>
        /// Paso de zoom
        /// </summary>
        public float ZoomStep
        {
            get { return _zoomStep; }
            set { _zoomStep = value; }
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

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            //Añadir eje de la gráfica
            this.AxisLine.Points.Clear();
            this.AxisLine.Points.Add(new Point(0, 0));
            this.AxisLine.Points.Add(new Point(0, this.ReportCanvas.ActualHeight));
            this.AxisLine.Points.Add(new Point(this.ReportCanvas.ActualWidth, this.ReportCanvas.ActualHeight));

            //Actualizar tamaños de las barras
            if (this._barsCollection.Count > 0)
            {
                double width = this.BarsWidth(this._barsCollection.Count);
                double height = this.BarsHeight();
                double barLeft = 0;
                foreach (Bar bar in _barsCollection)
                {
                    bar.Width = width;
                    bar.MaxHeight = height;
                    bar.Position = new Point(barLeft, 0);
                    barLeft += width;
                    bar.Update();
                }
            }

            //Actualizar las sombras
            if(this._shadowsShapesCollection.Count > 0)
            {
                this.UpdateShadows();
            }
        }

        private double BarsWidth(int dataCount)
        {
            return (this.ReportCanvas.ActualWidth) / dataCount;
        }

        private double BarsHeight()
        {
            return this.ReportCanvas.ActualHeight;
        }

        /// <summary>
        /// Muestra los datos
        /// </summary>
        /// <param name="dataSet">Conjunto de datos</param>
        /// <param name="tableName">Nombre de la tabla donde están los datos</param>
        /// <param name="valueColumnName">Nombre de la columna de los datos</param>
        public void AddBars(DataSet dataSet, string tableName, string valueColumnName)
        {
            if (dataSet.Tables[tableName].Rows.Count > 0)
            {
                List<float?> dataCollection = new List<float?>(dataSet.Tables[tableName].Rows.Count);
                dataCollection.AddRange(from DataRow row in dataSet.Tables[tableName].Rows
                                        select (float?) row[valueColumnName]);
                this.AddBars(dataCollection);
            }
        }

        /// <summary>
        /// Muestra los datos
        /// </summary>
        /// <param name="dataCollection">Colección con los datos</param>
        public void AddBars(IList<float?> dataCollection)
        {
            if (dataCollection.Count > 0)
            {
                double width = this.BarsWidth(dataCollection.Count);
                double maxHeight = this.BarsHeight();
                double barLeft = 0;
                foreach (float? data in dataCollection)
                {
                    //TODO Coger colores de la configuración
                    Bar bar = new Bar
                                  {
                                      FillBrush = Brushes.CornflowerBlue,
                                      StrokeBrush = Brushes.Blue,
                                      HighLightFillBrush = Brushes.Azure,
                                      HighLightStrokeBrush = Brushes.Cyan,
                                      Panel = this.ReportCanvas,
                                      ShowValueLabel = false,
                                      MaxHeight = (maxHeight > 0) ? maxHeight : 0,
                                      Position = new Point(barLeft, 0),
                                      Width = (width > 0) ? width : 0,
                                      Value = data,
                                      Opacity = this._barsOpacity
                                  };

                    bar.ValueChangedOnResizing += OnBarValueChangedOnResizing;
                    barLeft += width;
                    this._barsCollection.Add(bar);
                    if ((bar.Value != null) && (bar.Value > this.ChartMaxValue))
                    {
                        this._maxValueBar = bar;
                        this.ChartMaxValue = (float)bar.Value;
                    }
                }

                this.MaxValueLabel.Content = this.ChartMaxValue;
                this.UpdateBars();
            }
        }

        private void OnBarValueChangedOnResizing(object sender, EventArgs e)
        {
            Bar bar = sender as Bar;
            if (bar != null)
            {
                if ((bar.Value!=null)&&(bar.Value > this.ChartMaxValue))
                {
                    this.ChartMaxValue = (float)bar.Value;
                    this._maxValueBar = bar;
                    //Actualizar barras
                    foreach (Bar otherBar in _barsCollection)
                    {
                        if (otherBar != bar)
                        {
                            otherBar.MaxValue = this.ChartMaxValue;
                            otherBar.Update();
                        }
                    }
                }else if(bar == this._maxValueBar)
                {
                    this.ChartMaxValue = (bar.Value!=null)?(float)bar.Value:0f;
                    /*
                     * Comprobar que el valor máximo sigue siendo de
                     * la barra modificada o ahora es otra
                     */
                    foreach (Bar otherBar in _barsCollection)
                    {
                        if((otherBar.Value!=null)&&(otherBar.Value > this.ChartMaxValue))
                        {
                            this.ChartMaxValue = (float)otherBar.Value;
                            this._maxValueBar = otherBar;
                        }
                    }

                    //Actualizar barras
                    this.UpdateBars();
                }
            }
            this.UpdateShadows();
        }

        /// <summary>
        /// Actualiza todos los datos
        /// </summary>
        public void Update()
        {
            this.UpdateBars();
            this.UpdateShadows();
        }

        private void UpdateBars()
        {
            foreach (Bar bar in _barsCollection)
            {
                bar.MaxValue = this.ChartMaxValue;
                bar.Update();
            }
        }

        private void UpdateShadows()
        {
            foreach (string shadowName in this._shadowsDataCollection.Keys)
            {
                this.UpdateShadow(shadowName);
            }
        }

        private void UpdateShadow(string shadowName)
        {
            double lapWidth = this.BarsWidth(this._shadowsDataCollection[shadowName].Count);
            double height = this.BarsHeight();

            if ((lapWidth > 0) && (height > 0))
            {
                Polygon polygon = this._shadowsShapesCollection[shadowName];
                polygon.Points.Clear();

                double currentX = 0d;

                foreach (float value in this._shadowsDataCollection[shadowName])
                {
                    Point point1 = new Point
                                       {
                                           X = currentX,
                                           Y = height - (value*height/this.ChartMaxValue)
                                       };
                    currentX += lapWidth;
                    Point point2 = new Point
                                       {
                                           X = currentX,
                                           Y = point1.Y
                                       };
                    polygon.Points.Add(point1);
                    polygon.Points.Add(point2);
                }

                //Cierre de la figura
                polygon.Points.Add(new Point(this.ReportCanvas.ActualWidth, height));
                polygon.Points.Add(new Point(0d, height));
            }
        }

        /// <summary>
        /// Elimina todos los datos representados
        /// </summary>
        public void Clear()
        {
            this.ReportCanvas.Children.Clear();
            this.ReportCanvas.Children.Add(this.AxisLine);
            this._barsCollection.Clear();
            this._shadowsDataCollection.Clear();
            this._shadowsShapesCollection.Clear();
        }

        public void AddShadowFeatures(string name, ItemFeatures shadowFeatures)
        {
            if(!this._shadowFeaturesCollection.ContainsKey(name))
            {
                this._shadowFeaturesCollection.Add(name, shadowFeatures);
            }else
            {
                this._shadowFeaturesCollection[name] = shadowFeatures;
            }
        }

        public void AddShadow(string name, IList<float> data)
        {
            if (this._shadowFeaturesCollection.ContainsKey(name))
            {
                if (!this._shadowsDataCollection.ContainsKey(name))
                {
                    ItemFeatures shadowFeatures = this._shadowFeaturesCollection[name];
                    this._shadowsDataCollection.Add(name, data);
                    Polygon polygon = new Polygon
                                          {
                                              Stroke = shadowFeatures.Stroke,
                                              Fill = shadowFeatures.Fill,
                                              StrokeThickness = shadowFeatures.StrokeThickness,
                                              StrokeLineJoin = shadowFeatures.StrokeLineJoin,
                                              Opacity = shadowFeatures.Opacity,
                                              ClipToBounds = true
                                          };

                    this._shadowsShapesCollection.Add(name, polygon);
                    this.ReportCanvas.Children.Add(polygon);
                }
                else
                {
                    this._shadowsDataCollection[name] = data;
                }

                this.UpdateShadow(name);
            }
        }

        public void ZoomIn(float zoomStep)
        {
            this.ChartMaxValue -= this.ChartMaxValue*zoomStep;
            this._maxValueBar = null;
            this.Update();
        }

        public void ZoomOut(float zoomStep)
        {
            this.ChartMaxValue += this.ChartMaxValue*zoomStep;
            this._maxValueBar = null;
            this.Update();
        }

        private void OnZoomOutButtonClick(object sender, RoutedEventArgs e)
        {
            this.ZoomOut(this.ZoomStep);
        }

        private void OnZoomInButtonClick(object sender, RoutedEventArgs e)
        {
            this.ZoomIn(this.ZoomStep);
        }

        public void MarkExceedingBars(string dataCollectionName, ItemFeatures markedBarfeatures)
        {
            if (this._shadowsDataCollection.ContainsKey(dataCollectionName))
            {
                for (int i = 0; (i < this._barsCollection.Count) && (i < this._shadowsDataCollection[dataCollectionName].Count); i++)
                {
                    if (this._shadowsDataCollection[dataCollectionName][i] < this._barsCollection[i].Value)
                    {
                        Bar bar = this._barsCollection[i];
                        bar.FillBrush = markedBarfeatures.Fill;
                        bar.StrokeBrush = markedBarfeatures.Stroke;
                        bar.Opacity = markedBarfeatures.Opacity;
                    }
                }
            }
        }
    }
}
