using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DynamicReports.BarChart
{
    /// <summary>
    /// Barra de la gráfica
    /// </summary>
    internal class Bar
    {
        private const double grabMargin = 10d;

        private readonly Rectangle _rectangle = new Rectangle();
        private Label _valueLabel;

        private float? _value;
        private float _maxValue;

        private bool _highLight;
        private bool _hightLightBeforeMouseEnter;
        private Brush _strokeBrush;
        private Brush _highLightStrokeBrush;
        private Brush _fillBrush;
        private Brush _highLightFillBrush;

        private bool _resizing;
        private double _coordYReferenceOnResizing;

        /// <summary>
        /// Valor cambiado mientras se cambia el tamaño
        /// </summary>
        public event EventHandler ValueChangedOnResizing;
        /// <summary>
        /// Valor máximo cambiado mientras se cambia el tamaño
        /// </summary>
        public event EventHandler MaxValueChangedOnResizing;
        /// <summary>
        /// Se ha comenzado a cambiar el tamaño
        /// </summary>
        public event EventHandler ResizeBegins;
        /// <summary>
        /// Se ha terminado de cambiar el tamaño
        /// </summary>
        public event EventHandler ResizeFinish;

        /// <summary>
        /// Brocha con la que se pinta la línea de la barra
        /// </summary>
        public Brush StrokeBrush { 
            get { return this._strokeBrush; }
            set
            {
                this._strokeBrush = value;
                if(!this._highLight)
                {
                    this._rectangle.Stroke = this._strokeBrush;
                }
            }
        }
        /// <summary>
        /// Brocha con la que se pinta la línea de la barra en modo resaltado
        /// </summary>
        public Brush HighLightStrokeBrush
        {
            get { return this._highLightStrokeBrush; }
            set
            {
                this._highLightStrokeBrush = value;
                if(this._highLight)
                {
                    this._rectangle.Stroke = this._highLightStrokeBrush;
                }
            }
        }
        /// <summary>
        /// Brocha con la que se rellena la barra
        /// </summary>
        public Brush FillBrush { 
            get { return this._fillBrush; }
            set
            {
                this._fillBrush = value;
                if(!this._highLight)
                {
                    this._rectangle.Fill = this._fillBrush;
                }
            }
        }
        /// <summary>
        /// Brocha con la que se rellena la barra en modo resaltado
        /// </summary>
        public Brush HighLightFillBrush { 
            get { return this._highLightFillBrush; }
            set
            {
                this._highLightFillBrush = value;
                if(this._highLight)
                {
                    this._rectangle.Fill = this._highLightFillBrush;
                }
            } 
        }
        /// <summary>
        /// Obtiene o establece si está en modo resaltado
        /// </summary>
        public bool HighLight
        {
            get { return this._highLight; }
            set
            {
                this._highLight = value;
                if (this._highLight)
                {
                    this._rectangle.Fill = this.HighLightFillBrush;
                    this._rectangle.Stroke = this.HighLightStrokeBrush;
                }
                else
                {
                    this._rectangle.Fill = this.FillBrush;
                    this._rectangle.Stroke = this.StrokeBrush;
                }
            }
        }

        /// <summary>
        /// Posición dentro de la gráfica
        /// </summary>
        public Point Position { get; set; }
        /// <summary>
        /// Ancho de la barra
        /// </summary>
        public double Width { get; set; }
        /// <summary>
        /// Altura máxima que puede tener la barra
        /// </summary>
        public double MaxHeight { get; set; }
        /// <summary>
        /// Valor máximo que se corresponde con la altura máxima
        /// </summary>
        public float MaxValue
        {
            get
            {
                return this._maxValue;
            }
            set
            {
                this._maxValue = value;
                if(this.Resizing)
                {
                    this.OnEvent(this.MaxValueChangedOnResizing);
                }
            }
        }
        /// <summary>
        /// Valor que representa la barra
        /// </summary>
        public float? Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
                if(this.ShowValueLabel)
                {
                    this._valueLabel.Content = (this._value!=null)?((float)this._value).ToString("0.##"):"?";
                }
                if (this.Resizing)
                {
                    this.OnEvent(this.ValueChangedOnResizing);
                }
            }
        }

        public bool ShowValueLabel
        {
            get
            {
                return (this._valueLabel != null);
            }
            set
            {
                if(value != this.ShowValueLabel)
                {
                    if(value)
                    {
                        this._valueLabel = new Label();
                        this._valueLabel.HorizontalContentAlignment = HorizontalAlignment.Center;

                        if(this.Panel != null)
                        {
                            this.Panel.Children.Add(this._valueLabel);
                        }
                    }else
                    {
                        if((this.Panel != null) && (this.Panel.Children.Contains(this._valueLabel)))
                        {
                            this.Panel.Children.Remove(this._valueLabel);
                        }
                        this._valueLabel = null;
                    }
                }
            }
        }

        private bool Resizing
        {
            get
            {
                return this._resizing;
            }
            set
            {
                if(this._resizing != value)
                {
                    this._resizing = value;
                    this.OnEvent(this._resizing ? this.ResizeBegins : this.ResizeFinish);
                }
            }
        }

        public bool Resizable { get; set; }

        /// <summary>
        /// Opacidad de la barra
        /// </summary>
        public float Opacity { get; set; }
        
        /// Return Type: BOOL->int  
        ///X: int  
        ///Y: int  
        [DllImportAttribute("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int X, int Y);

        /// <summary>
        /// Constructor
        /// </summary>
        public Bar()
        {
            Resizable = false;
            this._rectangle.MouseEnter += OnRectangleMouseEnter;
            this._rectangle.MouseLeave += OnRectangleMouseLeave;
            this._rectangle.MouseMove += OnRectangleMouseMove;
            this._rectangle.MouseLeftButtonDown += OnRectangleMouseLeftButtonDown;
            this._rectangle.MouseLeftButtonUp += OnRectangleMouseLeftButtonUp;
            this._rectangle.SizeChanged += OnRectangleSizeChanged;
        }

        private void OnRectangleSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                this.SetValueFromHeight(e.NewSize.Height);
                this.LocateValueLabel();
            }
        }

        private void OnRectangleMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Resizing = false;
        }

        private void OnRectangleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double coordY = e.GetPosition(this._rectangle).Y;
            this.Resizing = ( coordY <= grabMargin);
            this._coordYReferenceOnResizing = e.GetPosition(this._rectangle).Y;
        }

        private void OnRectangleMouseMove(object sender, MouseEventArgs e)
        {
            if (this.Resizable)
            {
                double coordY = e.GetPosition(this._rectangle).Y;
                if (this.Resizing)
                {
                    double increment = this._coordYReferenceOnResizing - coordY;
                    this._rectangle.Height += increment;
                }
                else if ((coordY <= grabMargin) && (this._rectangle.Cursor != Cursors.SizeNS))
                {
                    this._rectangle.Cursor = Cursors.SizeNS;
                }
                else if ((coordY > grabMargin) && (this._rectangle.Cursor != Cursors.Arrow))
                {
                    this._rectangle.Cursor = Cursors.Arrow;
                }
            }
        }

        private void OnRectangleMouseLeave(object sender, MouseEventArgs e)
        {
            Point coords =  e.GetPosition(this._rectangle);
            if (this.Resizing && (coords.Y < 0))
            {
                double newHeight = this._rectangle.Height - coords.Y;
                this._rectangle.Height = newHeight;
                
                if (newHeight >= (this.MaxHeight - grabMargin))
                {
                    Point screenCoords = this._rectangle.PointToScreen(new Point(coords.X, grabMargin));
                    SetCursorPos((int)screenCoords.X, (int)screenCoords.Y);
                    this._coordYReferenceOnResizing = grabMargin;
                }else
                {
                    this._coordYReferenceOnResizing = 0;
                }
            }else
            {
                this.Resizing = false;
                this._rectangle.Cursor = Cursors.Arrow;
            }

            if (!this.Resizing && !this._hightLightBeforeMouseEnter)
            {
                this.HighLight = false;
            }
        }

        private void OnRectangleMouseEnter(object sender, MouseEventArgs e)
        {
            //Resaltar barra
            if (!this.Resizing)
            {
                this._hightLightBeforeMouseEnter = this._highLight;
                this.HighLight = true;
            }else if(e.LeftButton == MouseButtonState.Released)
            {
                this.Resizing = false;
            }
        }

        private void SetValueFromHeight(double height)
        {
            this.Value = (float)(height*this.MaxValue/this.MaxHeight);
            if(this.Value > this.MaxValue)
            {
                height = this.MaxHeight - grabMargin;
                this.MaxValue = (float)(this.Value * this.MaxHeight / height);
                this._rectangle.Height = height;
            }
        }

        /// <summary>
        /// Panel donde se mostrará la barra
        /// </summary>
        public Canvas Panel { get; set; }

        /// <summary>
        /// Actualiza su valor
        /// </summary>
        public void Update()
        {
            this._rectangle.Width = this.Width;
            this._rectangle.Height = ((this.Value!=null) && (this.MaxValue != 0))?((float)this.Value)*this.MaxHeight/this.MaxValue: 0d;
            this._rectangle.Opacity = this.Opacity;
            Canvas.SetBottom(this._rectangle, this.Position.Y);
            Canvas.SetLeft(this._rectangle, this.Position.X);
            if(this.Panel != null)
            {
                if (!this.Panel.Children.Contains(this._rectangle))
                {
                    this.Panel.Children.Add(this._rectangle);
                }
                if (this.ShowValueLabel && !this.Panel.Children.Contains(this._valueLabel))
                {
                    this.Panel.Children.Add(this._valueLabel);
                }
            }
        }

        private void LocateValueLabel()
        {
            if (this.ShowValueLabel)
            {
                this._valueLabel.Width = this._rectangle.Width;
                Canvas.SetBottom(this._valueLabel, Canvas.GetBottom(this._rectangle) + this._rectangle.Height);
                Canvas.SetLeft(this._valueLabel, Canvas.GetLeft(this._rectangle));
            }
        }

        private void OnEvent(EventHandler temp)
        {
            if(temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }
    }
}
