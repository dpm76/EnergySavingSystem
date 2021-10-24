using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using DynamicReports.BarChart;
using NLog;

namespace EnergySavingManager
{
    /// <summary>
    /// Ventana principal
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Número de intentos para conectarse con el servidor
        /// </summary>
        private const int NUM_TRIES_TO_CONNECT = 3;

        public MainWindow()
        {
            InitializeComponent();

            Manager.Default.SpanPicker = this.SpanPicker;
            Manager.Default.HistoricConsumptionViewer = this.HistoricViewer;
            Manager.Default.MarkedBarsFeatures = new ItemFeatures
                                                        {
                                                            Fill = Brushes.LightSalmon,
                                                            Stroke = Brushes.Red,
                                                            Opacity = 0.8f,
                                                            StrokeLineJoin = PenLineJoin.Miter,
                                                            StrokeThickness = 2d
                                                        };
            this.HistoricViewer.BarsOpacity = 0.7f;
            this.HistoricViewer.AddShadowFeatures(Manager.SAVING_LAYER_NAME, new ItemFeatures
                                                                                 {
                                                                                     Fill = Brushes.LightGreen,
                                                                                     Opacity = 1f,
                                                                                     Stroke = Brushes.LimeGreen,
                                                                                     StrokeLineJoin = PenLineJoin.Bevel,
                                                                                     StrokeThickness = 4d
                                                                                 });
            Manager.Default.SelectedConsumptionViewer = this.CurrentConsumptionViewer;
            Manager.Default.AlertsContainer = this.AlertsContainer;

            int triesleft = NUM_TRIES_TO_CONNECT;
            bool done = false;
            do
            {
                try
                {
                    --triesleft;
                    Dictionary<string, string[]> magnitudesBySources =
                        Manager.Default.GetConsumptionSourcesAndMagnitudes();
                    var enumerator = magnitudesBySources.GetEnumerator();
                    enumerator.MoveNext();
                    string sourceId = enumerator.Current.Key;
                    this.CurrentConsumptionViewer.LineGraph.DataMagnitudes = enumerator.Current.Value;
                    this.CurrentConsumptionViewer.SourceId = sourceId;

                    //Manager.Default.RealTimeConsumptionViewers.Add(this.CurrentConsumptionViewer.SourceId,
                    //                                               this.CurrentConsumptionViewer);

                    this.CostViewer.SourceId = sourceId;
                    Manager.Default.CostViewers.Add(sourceId, this.CostViewer);
                    Manager.Default.RegisterIntoConsumptionDataReader(this.CurrentConsumptionViewer.SourceId);

                    //foreach (KeyValuePair<string, string[]> pair in magnitudesBySources)
                    //{
                    //    RealTimeLineGraph realTimeViewer = new RealTimeLineGraph {Name = pair.Key, DataMagnitudes = pair.Value};
                    //    Manager.Default.RealTimeConsumptionViewers.Add(pair.Key, realTimeViewer);
                    //    this.RealTimeConsumptionGrid.Children.Add(realTimeViewer);
                    //    Manager.Default.RegisterIntoConsumptionDataReader(pair.Key);
                    //}

                    Manager.Default.Start();
                    done = true;
                }
                catch (Exception ex)
                {
                    if (triesleft == 0)
                    {
                        LogManager.GetCurrentClassLogger().FatalException(
                            "ERROR FATAL. No se ha podido comunicar con el servidor." + ex, ex);
                        MessageBox.Show(
                            "Asegúrese de que el sistema de lectura de consumo energético está funcionando.",
                            "No se ha podido iniciar la aplicación", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        Application.Current.Shutdown(-1);
                    }else
                    {
                        Thread.Sleep(5000);
                    }
                }
            } while (!done && (triesleft > 0));
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Manager.Default.Stop();
        }
    }
}
