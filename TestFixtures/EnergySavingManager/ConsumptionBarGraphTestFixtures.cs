using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnergySavingManager;
using NUnit.Framework;

namespace TestFixtures.EnergySavingManager
{
    [TestFixture]
    public class ConsumptionBarGraphTestFixtures
    {
        private ConsumptionBarGraph _uc;

        [SetUp]
        public void Init()
        {
            this._uc = new ConsumptionBarGraph();
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
                                         TestingControl = this._uc
                                     };
            window.ShowDialog();
        }
    }
}
