using System.Threading;
using System.Windows;
using EnergySavingManager.Alerts;
using NUnit.Framework;

namespace TestFixtures.EnergySavingManager
{
    [TestFixture]
    public class AlertItemTestFixtures
    {
        private AlertItem _uc;

        [SetUp]
        public void Init()
        {
            this._uc = new AlertItem();
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

        [Explicit]
        [Test]
        public void PressingButtonsTest()
        {
            DummyWindow window = new DummyWindow
            {
                TestingControl = this._uc
            };
            this._uc.ClosePressed += OnClosePressed;
            this._uc.GoPressed += OnGoPressed;
            window.ShowDialog();
        }

        private void OnGoPressed(object sender, System.EventArgs e)
        {
            MessageBox.Show("Ir");
        }

        private void OnClosePressed(object sender, System.EventArgs e)
        {
            MessageBox.Show("Cerrar");
        }

    }
}
