using System;
using EnergySavingManager.Alerts;
using NUnit.Framework;

namespace TestFixtures.EnergySavingManager
{
    [TestFixture]
    public class AlertsContainerTestFixtures
    {
        private NotificationsContainer _uc;

        [SetUp]
        public void Setup()
        {
            this._uc =  new NotificationsContainer();
        }

        [TearDown]
        public void Finish()
        {
            this._uc = null;
        }

        [Explicit]
        [Test]
        public void ShowTest()
        {
            DummyWindow window = new DummyWindow
                                     {
                                         TestingControl = this._uc,
                                         Width = 150,
                                         Height = 350
                                     };
            DateTime timeStamp = DateTime.UtcNow;
            for(int i=0;i<10;i++)
            {
                this._uc.AddAlert("Fake", timeStamp);
                timeStamp = timeStamp.AddMinutes(15);
            }
            window.ShowDialog();
        }
    }
}
