using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EnergySavingManager
{
    /// <summary>
    /// Permite seleccionar un periodo de tiempo
    /// </summary>
    public partial class SpanPicker : UserControl
    {
        private double _slotWidth;
        private short _numSlots = 30;
        private TimeRanges _timeRange = TimeRanges.Hour;
        private DateTime _selectionSince;
        private DateTime _selectionUntil;
        private DateTime _lastTimeInDial = DateTime.UtcNow;

        private readonly List<TextBlock> _timeLablesCollection;
        private DateTime[] _timeSlots;

        public event EventHandler SelectionChanged;
        public event EventHandler TimeRangeChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        public SpanPicker()
        {
            InitializeComponent();
            this._timeLablesCollection = new List<TextBlock>(this._numSlots);
            this._timeSlots = new DateTime[this._numSlots + 1];
        }

        public void Refresh()
        {
            this.Dispatcher.Invoke(new Action(() => { }));
        }

        /// <summary>
        /// Número de huecos para los item de tiempo
        /// </summary>
        public short NumSlots
        {
            get { return _numSlots; }
            set
            {
                this._numSlots = value;
                this._timeLablesCollection.Capacity = this._numSlots;
                this._timeSlots = new DateTime[this._numSlots + 1];
            }
        }

        /// <summary>
        /// Rango temporal que se está visualizando
        /// </summary>
        public TimeRanges TimeRange
        {
            get { return _timeRange; }
            set
            {
                if (this._timeRange != value)
                {
                    this._timeRange = value;
                    this.RedrawUiElements();
                    this.OnEvent(this.TimeRangeChanged);
                }
            }
        }

        /// <summary>
        /// Origen de la selección (UTC) 
        /// </summary>
        public DateTime SelectionSince
        {
            get { return _selectionSince; }
        }

        /// <summary>
        /// Final de la selección (UTC)
        /// </summary>
        public DateTime SelectionUntil
        {
            get { return _selectionUntil; }
        }

        /// <summary>
        /// Último momento en el ámbito del selector (UTC) 
        /// </summary>
        public DateTime LastTimeInDial
        {
            get { return _lastTimeInDial; }
            set
            {
                DateTime oldLastTimeViewed = this._lastTimeInDial;
                this._lastTimeInDial = value.ToUniversalTime();
                if (oldLastTimeViewed != this._lastTimeInDial)
                {
                    this.RedrawUiElements();
                }
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            this.RedrawUiElements();
            this.DockSelectionGroup();
            this.UpdateSelectedSpanTimeFromUi();
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            if(this.Parent != null)
            {
                this.Width = this.Height = double.NaN;
            }
        }

        private void RedrawUiElements()
        {
            this.SpanTimeCanvas.Children.Clear();
            this._timeLablesCollection.Clear();
            this.DrawSlots();
            this.TimeAxis.X1 = 0;
            this.SelectSpanLine.Y1 = 
                this.SelectSpanLine.Y2 =
                    this.TimeAxis.Y1 = 
                        this.TimeAxis.Y2 = 
                            this.SpanTimeCanvas.ActualHeight/2;
            this.TimeAxis.X2 = this.SpanTimeCanvas.ActualWidth;
            
            this.LeftTimeGrabber.Y1 = this.RightTimeGrabber.Y1 = this.SelectSpanLine.Y1 - 7;
            this.LeftTimeGrabber.Y2 = this.RightTimeGrabber.Y2 = this.SelectSpanLine.Y2 + 7;

            this.SelectedSpanRectangle.Height = this.SpanTimeCanvas.ActualHeight;

            this.SpanTimeCanvas.Children.Add(this.SelectedSpanRectangle);
            this.SpanTimeCanvas.Children.Add(this.TimeAxis);
            this.SpanTimeCanvas.Children.Add(this.SelectSpanLine);
            this.SpanTimeCanvas.Children.Add(this.LeftTimeGrabber);
            this.SpanTimeCanvas.Children.Add(this.RightTimeGrabber);
        }

        private void DrawSlots()
        {
            this._slotWidth = this.SpanTimeCanvas.ActualWidth/NumSlots;

            double rectangleX = 0;
            for(int i = 0; i < NumSlots; i++)
            {
                //Rectángulo
                Brush brush = ((i%2) == 0) ? Brushes.LightYellow : Brushes.WhiteSmoke;
                Border slot = new Border
                                          {
                                              Width = this._slotWidth,
                                              Height = this.SpanTimeCanvas.ActualHeight,
                                              BorderThickness = new Thickness(0d),
                                              Background = brush,
                                              Padding = new Thickness(0, 5, 0, 5)
                                          };
                Canvas.SetTop(slot, 0);
                Canvas.SetLeft(slot, rectangleX);
                this.SpanTimeCanvas.Children.Insert(0,slot);

                //Texto
                TextBlock textBlock = new TextBlock();
                textBlock.Text = "none";
                textBlock.LayoutTransform = new RotateTransform(-90);
                textBlock.VerticalAlignment = ((i%2) == 0)?VerticalAlignment.Top:VerticalAlignment.Bottom;
                
                slot.Child = textBlock;
                this._timeLablesCollection.Add(textBlock);

                rectangleX += this._slotWidth;
            }

            this.Populate();
        }

        private void OnTimeGrabberMouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.SizeWE;
        }

        private void OnTimeGrabberMouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                this.DockSelectionGroup();
                this.UpdateSelectedSpanTimeFromUi();
            }
        }

        private void OnTimeGrabberMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.DockSelectionGroup();
            this.UpdateSelectedSpanTimeFromUi();
        }

        private void OnTimeGrabberMouseMove(object sender, MouseEventArgs e)
        {
            if ((e.LeftButton == MouseButtonState.Pressed) && ((this.LeftTimeGrabber.X1 + this._slotWidth) <= this.RightTimeGrabber.X1))
            {
                Line grabber = (Line) sender;
                grabber.X1 = grabber.X2 = e.GetPosition(this.SpanTimeCanvas).X;
                this.AdjustSelectionGroupFromGrabbers();
            }
        }

        private void AdjustSelectionGroupFromGrabbers()
        {
            this.SelectSpanLine.X1 = this.LeftTimeGrabber.X1;
            this.SelectSpanLine.X2 = this.RightTimeGrabber.X1;

            this.SelectedSpanRectangle.Width = this.RightTimeGrabber.X1 - this.LeftTimeGrabber.X1;
            Canvas.SetLeft(this.SelectedSpanRectangle, this.LeftTimeGrabber.X1);
        }

        private void DockSelectionGroup()
        {
            this.LeftTimeGrabber.X1 = this.LeftTimeGrabber.X2 = this.GetDockingX(this.LeftTimeGrabber.X1);
            this.RightTimeGrabber.X1 = this.RightTimeGrabber.X2 = this.GetDockingX(this.RightTimeGrabber.X1);
            this.AdjustSelectionGroupFromGrabbers();
        }

        private double GetDockingX(double currentX)
        {
            return Math.Floor((currentX/this._slotWidth) + 0.5d) * this._slotWidth;
        }

        private void OnEvent(EventHandler eventHandler)
        {
            if(eventHandler != null)
            {
                eventHandler(this, EventArgs.Empty);
            }
        }

        private static string GetDateLabelByTimeRange(DateTime dateTime, TimeRanges timeRange)
        {
            string label;
            switch (timeRange)
            {
                case TimeRanges.Year:
                    label = dateTime.Year.ToString();
                    break;
                case TimeRanges.Month:
                    label = CultureInfo.CurrentUICulture.DateTimeFormat.GetMonthName(dateTime.Month);
                    break;
                case TimeRanges.Week:
                    label = string.Format("{0} {1}",
                                          CultureInfo.CurrentUICulture.DateTimeFormat.GetShortestDayName(
                                              dateTime.DayOfWeek),
                                          dateTime.ToShortDateString());
                    break;
                case TimeRanges.Day:
                    label = string.Format("{0} {1}",
                        CultureInfo.CurrentUICulture.DateTimeFormat.GetShortestDayName(dateTime.DayOfWeek),
                        dateTime.ToShortDateString());
                    break;
                default:
                    label = dateTime.ToShortTimeString();
                    break;
            }

            return label;
        }

        private void Populate()
        {
            TimeSpan slotTimeSpan;
            switch (this.TimeRange)
            {
                case TimeRanges.Month:
                    slotTimeSpan = new TimeSpan(30, 0, 0, 0);
                    break;
                case TimeRanges.Week:
                    slotTimeSpan = new TimeSpan(7, 0, 0, 0);
                    break;
                case TimeRanges.Day:
                    slotTimeSpan = new TimeSpan(1, 0, 0, 0);
                    break;
                case TimeRanges.Hour:
                    slotTimeSpan = new TimeSpan(1, 0, 0);
                    break;
                case TimeRanges.QHour:
                default:
                    slotTimeSpan = new TimeSpan(0,15,0);
                    break;
            }

            DateTime slotTimeEnding;

            if(this.TimeRange == TimeRanges.QHour)
            {
                slotTimeEnding = new DateTime(
                          this.LastTimeInDial.Year,
                          this.LastTimeInDial.Month,
                          this.LastTimeInDial.Day,
                          this.LastTimeInDial.Hour,
                          (this.LastTimeInDial.Minute / 15) * 15,
                          0, 0, DateTimeKind.Utc);
            }else if(this.TimeRange == TimeRanges.Hour)
            {
                slotTimeEnding = new DateTime(
                          this.LastTimeInDial.Year,
                          this.LastTimeInDial.Month,
                          this.LastTimeInDial.Day,
                          this.LastTimeInDial.Hour,
                          0, 0, 0, DateTimeKind.Utc);
            }else if(this.TimeRange == TimeRanges.Week)
            {
                DateTime lastDayInDial = new DateTime(
                    this.LastTimeInDial.Year,
                    this.LastTimeInDial.Month,
                    this.LastTimeInDial.Day,
                    0,0,0,DateTimeKind.Utc);
                slotTimeEnding =
                    lastDayInDial.AddDays((7 - (int)lastDayInDial.DayOfWeek + (int)CultureInfo.CurrentUICulture.DateTimeFormat.FirstDayOfWeek) % 7);

            }
            else
            {
                slotTimeEnding = new DateTime(this.LastTimeInDial.Year, this.LastTimeInDial.Month, this.LastTimeInDial.Day, 0,0,0,DateTimeKind.Utc);
            }

            DateTime slotTimeStamp;

            if (this.TimeRange != TimeRanges.Month)
            {
                slotTimeStamp = slotTimeEnding.Subtract(new TimeSpan(slotTimeSpan.Ticks*this._numSlots));
            }else
            {
                slotTimeStamp = CultureInfo.CurrentUICulture.Calendar.AddMonths(slotTimeEnding, -this._numSlots);
            }

            int slotIndex = 0;
            foreach (TextBlock timeLabel in this._timeLablesCollection)
            {
                this._timeSlots[slotIndex] = slotTimeStamp;
                DateTime slotLocalTimeStamp = slotTimeStamp.ToLocalTime();

                timeLabel.Text = GetDateLabelByTimeRange(slotLocalTimeStamp, this.TimeRange);
                
                //Cambio de periodo superior
                if ((slotIndex == 0) ||
                    ((this.TimeRange == TimeRanges.Month) && (slotLocalTimeStamp.Month == 1) && (slotLocalTimeStamp.Day == 1)) ||
                    ((this.TimeRange == TimeRanges.Week) && ((slotLocalTimeStamp.Day < 7))) ||
                    ((this.TimeRange == TimeRanges.Day) && ((slotLocalTimeStamp.Day == 1) || (slotLocalTimeStamp.DayOfWeek == CultureInfo.CurrentUICulture.DateTimeFormat.FirstDayOfWeek))) ||
                    (((this.TimeRange == TimeRanges.QHour) || (this.TimeRange == TimeRanges.Hour)) && (slotLocalTimeStamp.Hour == 0) && (slotLocalTimeStamp.Minute == 0)))
                {
                    Line divisionLine = new Line
                                            {
                                                Stroke = Brushes.Red,
                                                StrokeThickness = 2,
                                                Y1 = 0,
                                                Y2 = this.SpanTimeCanvas.ActualHeight
                                            };
                    divisionLine.X1 = divisionLine.X2 = slotIndex*this._slotWidth;
                    this.SpanTimeCanvas.Children.Add(divisionLine);

                    TextBlock divisionTextBlock = new TextBlock
                    {
                        Foreground = Brushes.Red,
                        LayoutTransform = new RotateTransform(-90)
                    };

                    if(this.TimeRange == TimeRanges.QHour)
                    {
                        divisionTextBlock.Text =
                            GetDateLabelByTimeRange(slotLocalTimeStamp, TimeRanges.Day);
                    }
                    else if (this.TimeRange != TimeRanges.Day)
                    {
                        divisionTextBlock.Text =
                            GetDateLabelByTimeRange(slotLocalTimeStamp,
                                                    this.TimeRange + 1);
                    }else if(slotLocalTimeStamp.Day == 1)
                    {
                        divisionTextBlock.Text = GetDateLabelByTimeRange(slotLocalTimeStamp, TimeRanges.Month);
                    }else
                    {
                        divisionTextBlock.Text = string.Empty;
                    }

                    Canvas.SetLeft(divisionTextBlock, slotIndex*this._slotWidth);
                    if ((slotIndex%2) == 0)
                    {
                        Canvas.SetBottom(divisionTextBlock,0);
                    }else
                    {
                        Canvas.SetTop(divisionTextBlock,0);
                    }

                    this.SpanTimeCanvas.Children.Add(divisionTextBlock);
                }

                if (this.TimeRange != TimeRanges.Month)
                {
                    slotTimeStamp = slotTimeStamp.Add(slotTimeSpan);
                }else if(slotTimeStamp.Month != 12)
                {
                    slotTimeStamp = new DateTime(
                        slotTimeStamp.Year,
                        slotTimeStamp.Month +1, 1);
                }else
                {
                    slotTimeStamp = new DateTime(
                        slotTimeStamp.Year +1,
                        1, 1);
                }
                slotIndex++;
            }
            this._timeSlots[slotIndex] = slotTimeStamp;
        }

        private void OnLeftButtonClick(object sender, RoutedEventArgs e)
        {
            switch (this.TimeRange)
            {
                case TimeRanges.Month:
                    this.LastTimeInDial = this.LastTimeInDial.AddMonths(-4);
                    break;
                case TimeRanges.Week:
                    this.LastTimeInDial = CultureInfo.CurrentUICulture.Calendar.AddWeeks(this.LastTimeInDial, -4);
                    break;
                case TimeRanges.Day:
                    this.LastTimeInDial = this.LastTimeInDial.AddDays(-4);
                    break;
                case TimeRanges.Hour:
                    this.LastTimeInDial = this.LastTimeInDial.AddHours(-4);
                    break;
                case TimeRanges.QHour:
                    this.LastTimeInDial = this.LastTimeInDial.AddHours(-1);
                    break;
            }
            this.RedrawUiElements();
            this.UpdateSelectedSpanTimeFromUi();
        }

        private void OnRightButtonClick(object sender, RoutedEventArgs e)
        {
            switch (this.TimeRange)
            {
                case TimeRanges.Month:
                    this.LastTimeInDial = this.LastTimeInDial.AddMonths(4);
                    break;
                case TimeRanges.Week:
                    this.LastTimeInDial = CultureInfo.CurrentUICulture.Calendar.AddWeeks(this.LastTimeInDial, 4);
                    break;
                case TimeRanges.Day:
                    this.LastTimeInDial = this.LastTimeInDial.AddDays(4);
                    break;
                case TimeRanges.Hour:
                    this.LastTimeInDial = this.LastTimeInDial.AddHours(4);
                    break;
                case TimeRanges.QHour:
                    this.LastTimeInDial = this.LastTimeInDial.AddHours(1);
                    break;
            }
            this.RedrawUiElements();
            this.UpdateSelectedSpanTimeFromUi();
        }

        private void OnSelectedSpanRectangleMouseEnter(object sender, MouseEventArgs e)
        {
            this.SelectSpanRectangleCursor(e.GetPosition(this.SelectedSpanRectangle).X);
        }

        private bool _cursorOverSpanRectangleAtLeft;

        private void SelectSpanRectangleCursor(double x)
        {
            if (x < 16)
            {
                this.Cursor = Cursors.SizeWE;
                _cursorOverSpanRectangleAtLeft = true;
            }
            else if (x > (this.SelectedSpanRectangle.ActualWidth - 16))
            {
                this.Cursor = Cursors.SizeWE;
                _cursorOverSpanRectangleAtLeft = false;
            }
            else
            {
                this.Cursor = Cursors.ScrollWE;
            }

        }

        private void OnSelectedSpanRectangleMouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                this.DockSelectionGroup();
                this.UpdateSelectedSpanTimeFromUi();
            }
        }

        private Point _lastCoord;
        private void OnSelectedSpanRectangleMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentCoord = e.GetPosition(this.SpanTimeCanvas);
                double increment = currentCoord.X - _lastCoord.X;

                if((this.Cursor == Cursors.ScrollWE) || this._cursorOverSpanRectangleAtLeft)
                {
                    this.LeftTimeGrabber.X1 = this.LeftTimeGrabber.X2 += increment;
                }

                if((this.Cursor == Cursors.ScrollWE) || !this._cursorOverSpanRectangleAtLeft)
                {
                    this.RightTimeGrabber.X1 = this.RightTimeGrabber.X2 += increment;
                }

                if (this.LeftTimeGrabber.X1 < 0)
                {
                    this.LeftTimeGrabber.X1 = this.LeftTimeGrabber.X2 = 0;
                }

                if (this.RightTimeGrabber.X1 > this.SpanTimeCanvas.ActualWidth)
                {
                    this.RightTimeGrabber.X1 = this.RightTimeGrabber.X2 = this.SpanTimeCanvas.ActualWidth;
                }

                this.AdjustSelectionGroupFromGrabbers();
                this._lastCoord = currentCoord;
            }else
            {
                this.SelectSpanRectangleCursor(e.GetPosition(this.SelectedSpanRectangle).X);
            }
        }

        private void OnSelectedSpanRectangleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _lastCoord = e.GetPosition(this.SpanTimeCanvas);
        }

        private void OnSelectedSpanRectangleMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.DockSelectionGroup();
            this.UpdateSelectedSpanTimeFromUi();
        }

        private void UpdateSelectedSpanTimeFromUi()
        {
            DateTime oldSelectionSince = this._selectionSince;
            DateTime oldSelectionUntil = this._selectionUntil;

            this._selectionSince = this._timeSlots[0]; //this._timeSlots[(int) Math.Floor((this.LeftTimeGrabber.X1/this._slotWidth) + 0.5d)];
            this._selectionUntil = this._timeSlots[this._timeSlots.Length - 1]; //this._timeSlots[(int) Math.Floor((this.RightTimeGrabber.X1/this._slotWidth) + 0.5d)];

            this.UpdateSelectedSpanTimeLabel();

            if ((oldSelectionSince != this._selectionSince) || (oldSelectionUntil != this._selectionUntil))
            {
                this.OnEvent(this.SelectionChanged);
            }
        }

        /// <summary>
        /// Asigna el periodo seleccionado
        /// </summary>
        /// <param name="since">Desde</param>
        /// <param name="until">Hasta</param>
        public void SetSelectedSpanTime(DateTime since, DateTime until)
        {
            DateTime sinceUtc = since.ToUniversalTime();
            DateTime untilUtc = until.ToUniversalTime();

            if(untilUtc > this.LastTimeInDial)
            {
                this.LastTimeInDial = untilUtc;
            }

            if(sinceUtc < this._timeSlots[0])
            {
                this.LastTimeInDial = this.LastTimeInDial.Subtract(this._timeSlots[0].Subtract(sinceUtc));
            }

            int sinceIndex = this.TimeSlotIndex((sinceUtc < this._timeSlots[0]) ? this._timeSlots[0] : sinceUtc);
            int untilIndex = this.TimeSlotIndex((untilUtc > this._timeSlots[this._numSlots]) ? this._timeSlots[this._numSlots] : untilUtc);

            this.LeftTimeGrabber.X1 = this.LeftTimeGrabber.X2 = this._slotWidth*sinceIndex;
            this.RightTimeGrabber.X1 = this.RightTimeGrabber.X2 = this._slotWidth*untilIndex;
            this.AdjustSelectionGroupFromGrabbers();

            this._selectionSince = this._timeSlots[sinceIndex];
            this._selectionUntil = this._timeSlots[untilIndex];

            this.UpdateSelectedSpanTimeLabel();

            this.OnEvent(this.SelectionChanged);
        }

        private void UpdateSelectedSpanTimeLabel()
        {
            this.SelectedSpanTimeLabel.Content = string.Format("{0} - {1}", this._selectionSince.ToLocalTime().ToString("dd/MM/yyyy HH:mm"), this._selectionUntil.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));
        }

        private int TimeSlotIndex(DateTime dateTime)
        {
            int i, j;
            i = 0;
            j = this._numSlots;
            while((i + 1) < j)
            {
                int k = i + ((j - i)/2);
                if(this._timeSlots[k] < dateTime)
                {
                    i = k + 1;
                }else
                {
                    j = k;
                }
            }

            return i;
        }

        private void OnHourButtonClick(object sender, RoutedEventArgs e)
        {
            this.TimeRange = TimeRanges.Hour;
        }

        private void OnDayButtonClick(object sender, RoutedEventArgs e)
        {
            this.TimeRange = TimeRanges.Day;
        }

        private void OnWeekButtonClick(object sender, RoutedEventArgs e)
        {
            this.TimeRange = TimeRanges.Week;
        }

        private void OnMonthButtonClick(object sender, RoutedEventArgs e)
        {
            this.TimeRange = TimeRanges.Month;
        }
    }
}
