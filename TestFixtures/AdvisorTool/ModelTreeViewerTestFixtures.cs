using System.Collections.Generic;
using System.Windows;
using AdvisorTool;
using ConsumptionModelling.Model;
using NUnit.Framework;

namespace TestFixtures.AdvisorTool
{
    [TestFixture]
    public class ModelTreeViewerTestFixtures
    {
        private ModelTreeViewer _uc;

        [SetUp]
        public void Init()
        {
            ModelTreeViewer.YearLevel = CreateModel();
            this._uc = new ModelTreeViewer();
            this._uc.Selected += OnModelSelected;
        }

        private static void OnModelSelected(object sender, ModelEventArgs e)
        {
            MessageBox.Show(string.Format("Seleccionado {0}", e.Model));
        }

        [TearDown]
        public void Finish()
        {
            this._uc = null;
        }

        [Explicit]
        [Test]
        public void ShowModelTreeViewerControl()
        {
            DummyWindow window = new DummyWindow
            {
                TestingControl = this._uc
            };

            window.ShowDialog();
        }

        private static YearLevel CreateModel()
        {
            return new YearLevel
                       {
                           ConcreteDayModels =
                               new List<ConcreteDay>(new[]
                                                         {
                                                             new ConcreteDay
                                                                 {
                                                                     Day = 21,
                                                                     Month = 6,
                                                                     Ranges =
                                                                         new[]
                                                                             {
                                                                                 new ConsumptionRange
                                                                                     {Since = 0, Until = 24, Value = 7f}
                                                                             }
                                                                 }
                                                         }),
                           DefaultDayModel = {Ranges = new[] {new ConsumptionRange {Since = 0, Until = 24, Value = 3f}}},
                           MonthModels = new List<MonthLevel>(new[]
                                                                  {
                                                                      new MonthLevel
                                                                          {
                                                                              Month = 2,
                                                                              HolidayModel = new HolidayLevel
                                                                                                 {
                                                                                                     Ranges = new[]
                                                                                                                  {
                                                                                                                      new ConsumptionRange
                                                                                                                          {
                                                                                                                              Since
                                                                                                                                  =
                                                                                                                                  0,
                                                                                                                              Until
                                                                                                                                  =
                                                                                                                                  24,
                                                                                                                              Value
                                                                                                                                  =
                                                                                                                                  2f
                                                                                                                          }
                                                                                                                  }
                                                                                                 }
                                                                          }
                                                                  }),
                           WorkingModel = new WorkingLevel
                                              {
                                                  Ranges = new[]
                                                               {
                                                                   new ConsumptionRange
                                                                       {Since = 0, Until = 8, Value = 1f},
                                                                   new ConsumptionRange
                                                                       {Since = 8, Until = 13, Value = 6f},
                                                                   new ConsumptionRange
                                                                       {Since = 13, Until = 15, Value = 4f},
                                                                   new ConsumptionRange
                                                                       {Since = 15, Until = 18, Value = 6f},
                                                                   new ConsumptionRange
                                                                       {Since = 18, Until = 20, Value = 3f},
                                                                   new ConsumptionRange
                                                                       {Since = 20, Until = 24, Value = 1f}
                                                               }
                                              }
                       };
        }
    }
}
