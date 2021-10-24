using System.Threading;
using System.Collections.Generic;
using System.Windows.Media;
using DynamicReports.BarChart;
using NUnit.Framework;

namespace TestFixtures.DynamicReports.BarChart
{
    [TestFixture]
    public class BarGraphTestFixtures
    {
        private BarGraph _uc;

        [SetUp]
        public void Init()
        {
            this._uc = new BarGraph();
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
                                         TestingControl = this._uc
                                     };
            window.Show();
            Thread.Sleep(3000);
            window.Close();
        }

        [Test]
        public void ShowDataTest()
        {
            float?[] dataCollection = new float?[50];
            Randomizer rand = new Randomizer();

            //Preparar datos
            for(int i = 0; i < dataCollection.Length; i++)
            {
                dataCollection[i] = (float)(rand.NextDouble() * 100d);
            }

            //Mostrar
            DummyWindow window = new DummyWindow
            {
                TestingControl = this._uc
            };
            
            this._uc.AddBars(new List<float?>(dataCollection));
            window.Show();
            Thread.Sleep(3000);
            window.Close();
        }

        [Test]
        public void ShowDataWithShadowsTest()
        {
            float?[] dataCollection = new float?[20];
            float[] shadow1DataCollection = new float[20];
            float[] shadow2DataCollection = new float[20];
            Randomizer rand = new Randomizer();

            //Preparar datos
            for (int i = 0; i < dataCollection.Length; i++)
            {
                dataCollection[i] = (float)(rand.NextDouble() * 100d);
                shadow1DataCollection[i] = (float)(rand.NextDouble() * 100d);
                shadow2DataCollection[i] = (float)(rand.NextDouble() * 100d);
            }

            //Mostrar
            DummyWindow window = new DummyWindow
            {
                TestingControl = this._uc
            };

            this._uc.AddShadowFeatures("shadow1", new ItemFeatures
                                                      {
                                                          Fill = Brushes.Green,
                                                          Stroke = Brushes.LightGreen,
                                                          StrokeLineJoin = PenLineJoin.Bevel,
                                                          StrokeThickness = 2d,
                                                          Opacity = 0.3f
                                                      });
            this._uc.AddShadowFeatures("shadow2", new ItemFeatures
            {
                Fill = Brushes.Red,
                Stroke = Brushes.LightSalmon,
                StrokeLineJoin = PenLineJoin.Bevel,
                StrokeThickness = 2d,
                Opacity = 0.3f
            });

            this._uc.BarsOpacity = 0.8f;
            this._uc.AddShadow("shadow1", shadow1DataCollection);
            this._uc.AddShadow("shadow2", shadow2DataCollection);
            this._uc.AddBars(new List<float?>(dataCollection));

            window.ShowDialog();

            //window.Show();
            //Thread.Sleep(3000);
            //window.Close();
        }
    }
}
