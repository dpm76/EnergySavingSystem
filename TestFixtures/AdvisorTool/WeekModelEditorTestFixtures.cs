using System;
using System.Collections.Generic;
using System.Windows;
using AdvisorTool;
using ConsumptionModelling.Model;
using NUnit.Framework;

namespace TestFixtures.AdvisorTool
{
    class WeekModelEditorTestFixtures
    {
        private WeekModelEditor _uc;

        [SetUp]
        public void Init()
        {
            this._uc = new WeekModelEditor();
            this._uc.WeekModel = CreateModel();
            this._uc.DaySelected += OnDaySelected;
        }

        private static void OnDaySelected(object sender, DayModelEventArgs e)
        {
            MessageBox.Show(string.Format("Seleccionado {0}", e.DayModel));
        }

        [TearDown]
        public void Finish()
        {
            this._uc = null;
        }

        [Explicit]
        [Test]
        public void ShowControlTest()
        {
            DummyWindow window = new DummyWindow
            {
                TestingControl = this._uc
            };

            window.ShowDialog();
        }

        private static WeekLevel CreateModel()
        {
            return new WeekLevel
                       {
                           MonthWeekNum = 0,
                           DefaultDayModel =
                               new DayLevel {Ranges = new[] {new ConsumptionRange {Since = 0, Until = 24, Value = 3f}}},
                           HolidayModel = new HolidayLevel
                                              {
                                                  Ranges = new[]
                                                               {
                                                                   new ConsumptionRange
                                                                       {
                                                                           Since = 0,
                                                                           Until = 24,
                                                                           Value = 2f
                                                                       }
                                                               }
                                              },
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
                                              },
                           WeekDayModels = new List<WeekDayLevel>
                               {
                                   new WeekDayLevel
                                       {
                                           DayOfWeek = DayOfWeek.Friday,
                                           Ranges = new[]
                                                        {
                                                            new ConsumptionRange {Since = 0, Until = 10, Value = 3f},
                                                            new ConsumptionRange {Since = 10, Until = 16, Value = 6f},
                                                            new ConsumptionRange {Since = 16, Until = 24, Value = 4f}
                                                        }
                                       },
                                   new WeekDayLevel
                                       {
                                           DayOfWeek = DayOfWeek.Thursday,
                                           Ranges = new[]
                                                        {
                                                            new ConsumptionRange {Since = 0, Until = 10, Value = 3f},
                                                            new ConsumptionRange {Since = 10, Until = 16, Value = 6f},
                                                            new ConsumptionRange {Since = 16, Until = 24, Value = 4f}
                                                        }
                                       }
                               }
                       };
        }
    }
}
