using EnergySavingManager;
using NUnit.Framework;

namespace TestFixtures.EnergySavingManager
{
    [TestFixture]
    public class CostViewerTestFixtures
    {
        private CostViewer _uc;

        [SetUp]
        public void Init()
        {
            this._uc = new CostViewer();
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

            window.ShowDialog();
        }
    }
}
