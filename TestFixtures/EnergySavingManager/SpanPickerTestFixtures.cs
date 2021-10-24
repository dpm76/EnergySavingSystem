using System;
using System.Threading;
using System.Windows;
using EnergySavingManager;
using NUnit.Framework;

namespace TestFixtures.EnergySavingManager
{
    [TestFixture]
    public class SpanPickerTestFixtures
    {
        private SpanPicker _uc;

        [SetUp]
        public void Init()
        {
            this._uc = new SpanPicker();
        }

        [TearDown]
        public void Finish()
        {
            this._uc = null;
        }

        [Explicit]
        [Test]
        public void ShowTestExplicit()
        {
            DummyWindow window = new DummyWindow
                                     {
                                         TestingControl = this._uc,
                                         Width = 800,
                                         Height = 150
                                     };

            window.ShowDialog();
        }

        [Explicit]
        [Test]
        public void DayligthSavingTestExplicit()
        {
            DummyWindow window = new DummyWindow
            {
                TestingControl = this._uc,
                Width = 800,
                Height = 150
            };

            this._uc.LastTimeInDial = new DateTime(2011, 3, 27, 6, 0, 0);

            window.ShowDialog();
        }

        [Explicit]
        [Test]
        public void DayligthSavingTestInQHourModeExplicit()
        {
            DummyWindow window = new DummyWindow
            {
                TestingControl = this._uc,
                Width = 800,
                Height = 150
            };

            this._uc.TimeRange = TimeRanges.QHour;
            this._uc.LastTimeInDial = new DateTime(2011, 3, 27, 6, 0, 0);

            window.ShowDialog();
        }

        [Explicit]
        [Test]
        public void SelectionChangeNotificationTest()
        {
            DummyWindow window = new DummyWindow
            {
                TestingControl = this._uc,
                Width = 800,
                Height = 150
            };

            this._uc.SelectionChanged += OnSelectionChanged;

            window.ShowDialog();
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            MessageBox.Show(string.Format("Selección {0} -> {1}", this._uc.SelectionSince.ToLocalTime(), this._uc.SelectionUntil.ToLocalTime()));
        }
        
        [Test]
        public void SetSelectionSpanTest()
        {
            DummyWindow window = new DummyWindow
            {
                TestingControl = this._uc,
                Width = 800,
                Height = 150
            };

            this._uc.LastTimeInDial = new DateTime(2011, 2, 4, 13,45,0);
            window.Show();
            Thread.Sleep(3000);
            this._uc.SetSelectedSpanTime(new DateTime(2010, 2,4), new DateTime(2011, 2, 4, 10, 0,0));
            this._uc.Refresh();
            Thread.Sleep(3000);
            this._uc.SetSelectedSpanTime(new DateTime(2011, 4,2,10,0,0), new DateTime(2011, 4,2,13,0,0));
            this._uc.Refresh();
            Thread.Sleep(3000);
            window.Close();
        }

    }
}
