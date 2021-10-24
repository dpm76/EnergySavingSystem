using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Media;
using DynamicReports.RealTimeConsumption;
using NUnit.Framework;

namespace TestFixtures.DynamicReports.RealTimeConsumption
{
    [TestFixture]
    public class RealTimeLineGraphTestFixtures
    {
        private RealTimeLineGraph _uc;

        [SetUp]
        public void Init()
        {
            this._uc = new RealTimeLineGraph();
        }

        [TearDown]
        public void Finish()
        {
            this._uc = null;
        }

        [Test]
        public void ShowTest()
        {
            DummyWindow window = new DummyWindow
            {
                TestingControl = this._uc,
            };

            window.Show();
            Thread.Sleep(3000);
            window.Close();
        }

        [Explicit]
        [Test]
        //No funciona. El proceso tiene que ser STA
        public void PlotTestAsync()
        {
            DummyWindow window = new DummyWindow
                                     {
                                         TestingControl = this._uc,
                                         Process = PlotTestProcess
                                     };

            window.DoProcess();
        }
        
        [Test]
        public void PlotTest()
        {
            DummyWindow window = new DummyWindow
            {
                TestingControl = this._uc
            };
            
            window.Show();
            this.PlotTestProcess(null);
            Thread.Sleep(3000);
            window.Close();
        }
        
        private object PlotTestProcess(object args)
        {
            this._uc.DataMagnitudes = new []{"source1", "source2", "source3"};
            this._uc.SetBrush("source1", Brushes.OrangeRed);
            this._uc.SetBrush("source2", Brushes.DarkGreen);
            this._uc.SetBrush("source3", Brushes.YellowGreen);
            Random rnd = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < 125; i++)
            {
                RealTimeData data = new RealTimeData
                                        {
                                            TimeStamp = DateTime.UtcNow,
                                            DataCollection = new Dictionary<string, float>()
                                        };
                data.DataCollection.Add("source1", (float)(rnd.NextDouble() * 1000d));
                data.DataCollection.Add("source2", (float)(rnd.NextDouble() * 100d));
                data.DataCollection.Add("source3", 0f);
                this._uc.PumpData(data);
                this._uc.Refresh();
                Thread.Sleep(100);
            }
            for (int i = 0; i < 125; i++)
            {
                RealTimeData data = new RealTimeData
                {
                    TimeStamp = DateTime.UtcNow,
                    DataCollection = new Dictionary<string, float>()
                };
                data.DataCollection.Add("source1", (float)(rnd.NextDouble() * 10d));
                data.DataCollection.Add("source2", (float)(rnd.NextDouble() * 100d));
                data.DataCollection.Add("source3", 0f);
                this._uc.PumpData(data);
                this._uc.Refresh();
                Thread.Sleep(100);
            }

            return null;
        }
    }
}
