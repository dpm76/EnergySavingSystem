using NUnit.Framework;

namespace TestFixtures
{
    [TestFixture]
    public class DummyWindowTestFixtures
    {
        [Test]
        public void ShowTest()
        {
            DummyWindow window = new DummyWindow {Process = TestProcess};
            int result = (int)window.DoProcess(new [] {3, 5});

            Assert.AreEqual(8, result);
        }

        private static object TestProcess(object args)
        {
            int[] elements = args as int[];
            int result = 0;

            if (elements != null)
            {
                foreach (int element in elements)
                {
                    result += element;
                }
            }

            return result;
        }
    }
}
